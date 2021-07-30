using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Interface.Dtos;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class ObjectiveIssueConverter : IConverter<ObjectiveExternalDto, Issue>
    {
        private readonly Bim360Snapshot snapshot;
        private readonly IConverter<ObjectiveStatus, Status> statusConverter;
        private readonly IssuesService issuesService;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly ItemsService itemsService;

        public ObjectiveIssueConverter(
            Bim360Snapshot snapshot,
            IConverter<ObjectiveStatus, Status> statusConverter,
            IssuesService issuesService,
            ItemsSyncHelper itemsSyncHelper,
            ItemsService itemsService)
        {
            this.snapshot = snapshot;
            this.statusConverter = statusConverter;
            this.issuesService = issuesService;
            this.itemsSyncHelper = itemsSyncHelper;
            this.itemsService = itemsService;
        }

        public async Task<Issue> Convert(ObjectiveExternalDto objective)
        {
            Issue exist = null;
            var project = GetProjectSnapshot(objective);
            if (objective.ExternalID != null)
                exist = await issuesService.GetIssueAsync(project.IssueContainer, objective.ExternalID);

            string type, subtype, targetUrn;
            string[] permittedAttributes = null;
            Status[] permittedStatuses = null;
            int? startingVersion;
            Vector3? globalOffset = null;
            var typeSnapshot = GetIssueTypes(project, objective);

            if (exist == null)
            {
                (type, subtype) = (typeSnapshot.ParentTypeID, typeSnapshot.SubtypeID);
                (targetUrn, startingVersion) = await GetTarget(objective, project);
            }
            else
            {
                type = exist.Attributes.NgIssueTypeID;
                subtype = exist.Attributes.NgIssueSubtypeID;

                if (typeSnapshot.ParentTypeID == type)
                    subtype = typeSnapshot.SubtypeID;

                permittedAttributes = exist.Attributes.PermittedAttributes;
                permittedStatuses = exist.Attributes.PermittedStatuses;
                targetUrn = exist.Attributes.TargetUrn;
                startingVersion = exist.Attributes.StartingVersion;
                globalOffset = exist.Attributes.PushpinAttributes?.ViewerState?.GlobalOffset;
            }

            var result = new Issue
            {
                ID = objective.ExternalID,
                Attributes = new Issue.IssueAttributes
                {
                    Title = objective.Title,
                    Description = objective.Description,
                    Status = await statusConverter.Convert(objective.Status),
                    AssignedTo = GetDynamicField(objective.DynamicFields, x => x.AssignedTo),
                    CreatedAt = ConvertToNullable(objective.CreationDate),
                    DueDate = ConvertToNullable(objective.DueDate),
                    LocationDescription = GetDynamicField(objective.DynamicFields, x => x.LocationDescription),
                    RootCause = GetDynamicField(objective.DynamicFields, x => x.RootCause),
                    Answer = GetDynamicField(objective.DynamicFields, x => x.Answer),
                    PushpinAttributes =
                        await GetPushpinAttributes(objective.Location, project.IssueContainer, targetUrn, globalOffset),
                    NgIssueTypeID = type,
                    NgIssueSubtypeID = subtype,
                    PermittedAttributes = permittedAttributes,
                    PermittedStatuses = permittedStatuses,
                    TargetUrn = targetUrn,
                    StartingVersion = startingVersion,
                },
            };

            SetBimElements(objective, result);
            return result;
        }

        private static DateTime? ConvertToNullable(DateTime dateTime)
            => dateTime == default ? null : dateTime;

        private static string GetDynamicField(
            IEnumerable<DynamicFieldExternalDto> dynamicFields,
            Expression<Func<Issue.IssueAttributes, object>> property)
        {
            var fieldID = DataMemberUtilities.GetPath(property);
            var field = dynamicFields?.FirstOrDefault(f => f.ExternalID == fieldID);
            return field?.Value;
        }

        private static void SetBimElements(ObjectiveExternalDto objective, Issue result)
        {
            if (objective.BimElements is not { Count: > 0 } || objective.Location == null)
                return;

            result.Attributes.PushpinAttributes ??= new Issue.PushpinAttributes();
            result.Attributes.PushpinAttributes.ViewerState ??= new Issue.ViewerState();
            result.Attributes.PushpinAttributes.ViewerState.OtherInfo = GetOtherInfo(objective);
        }

        private static object GetOtherInfo(ObjectiveExternalDto objective)
            => new
            {
                objective.BimElements,
            };

        private async Task<Issue.PushpinAttributes> GetPushpinAttributes(
            LocationExternalDto locationDto,
            string containerID,
            string targetUrn,
            Vector3? globalOffset = default)
        {
            if (locationDto == null)
                return null;

            var offset = globalOffset ?? await GetGlobalOffset(containerID, targetUrn);
            var target = locationDto.Location.ToVector().ToFeet().ToXZY();
            var eye = locationDto.CameraPosition.ToVector().ToFeet().ToXZY();

            return new Issue.PushpinAttributes
            {
                Location = target - offset,
                ViewerState = new Issue.ViewerState
                {
                    Viewport = new Issue.Viewport
                    {
                        AspectRatio = 60,
                        Eye = eye,
                        Up = (target - eye).GetUpwardVector(),
                        Target = target,
                    },
                    GlobalOffset = offset,
                },
            };
        }

        private IssueTypeSnapshot GetIssueTypes(ProjectSnapshot projectSnapshot, ObjectiveExternalDto obj)
        {
            var dynamicField = obj.DynamicFields.First(d => d.ExternalID == new TypeSubtypeEnumCreator().EnumExternalID);
            var deserializedIDs = DynamicFieldUtilities.DeserializeID<(string parentTypeID, string subtypeID)>(dynamicField.Value).ToArray();
            var subtypeDictionary = deserializedIDs.ToDictionary(x => x.subtypeID);
            var type = projectSnapshot.IssueTypes.FirstOrDefault(x => subtypeDictionary.ContainsKey(x.Value.SubtypeID))
               .Value;

            if (type == null)
            {
                var typeLookup = deserializedIDs.ToLookup(x => x.parentTypeID);
                type = projectSnapshot.IssueTypes.FirstOrDefault(
                            x => x.Value.SubTypeIsType && typeLookup.Contains(x.Value.ParentTypeID))
                       .Value ??
                    projectSnapshot.IssueTypes
                       .FirstOrDefault(x => typeLookup.Contains(x.Value.ParentTypeID))
                       .Value ??
                    projectSnapshot.IssueTypes.First().Value;
            }

            return type;
        }

        private ProjectSnapshot GetProjectSnapshot(ObjectiveExternalDto obj)
            => snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID)
               .Value;

        private async Task<(string item, int? version)> GetTarget(ObjectiveExternalDto obj, ProjectSnapshot project)
        {
            async Task<Version> GetVersion(string itemID, long size)
            {
                if (itemID == null)
                    return null;

                var versions = (await itemsService.GetVersions(project.ID, itemID))
                   .OrderByDescending(x => x.Attributes.VersionNumber ?? 0)
                   .ToArray();
                return versions.FirstOrDefault(x => x.Attributes.StorageSize == size) ?? versions.First();
            }

            if (obj.Location != null)
            {
                Item item;
                Version version;

                if (!project.Items.TryGetValue(obj.Location.Item.ExternalID, out var itemSnapshot))
                    itemSnapshot = project.FindItemByName(obj.Location.Item.FileName);

                if (itemSnapshot == null)
                {
                    (item, version) = await itemsSyncHelper.PostItem(project, obj.Location.Item);
                }
                else
                {
                    item = itemSnapshot.Entity;
                    var size = new FileInfo(obj.Location.Item.FullPath).Length;
                    version = itemSnapshot.Version.Attributes.StorageSize == size
                        ? itemSnapshot.Version
                        : await GetVersion(item?.ID, size);
                }

                return (item?.ID, version?.Attributes.VersionNumber);
            }

            if (obj.BimElements is { Count: > 0 })
            {
                foreach (var file in obj.BimElements
                   .GroupBy(x => x.ParentName, (name, elements) => (name, count: elements.Count()))
                   .OrderByDescending(x => x.count)
                   .Select(x => x.name))
                {
                    var itemSnapshot = project.FindItemByName(file, true);

                    if (itemSnapshot != null)
                        return (itemSnapshot.Entity?.ID, itemSnapshot.Version?.Attributes.VersionNumber);
                }
            }

            return default;
        }

        private async Task<Vector3> GetGlobalOffset(string containerID, string targetUrn)
        {
            if (!string.IsNullOrWhiteSpace(targetUrn))
            {
                var filter = new Filter(
                    DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.TargetUrn),
                    targetUrn);
                var other = await issuesService.GetIssuesAsync(
                    containerID,
                    new[] { filter });
                var withGlobalOffset = other.FirstOrDefault(
                    x => (x.Attributes.PushpinAttributes?.ViewerState?.GlobalOffset ?? Vector3.Zero) != Vector3.Zero);
                var offset = withGlobalOffset?.Attributes.PushpinAttributes.ViewerState.GlobalOffset ?? Vector3.Zero;
                return offset;
            }

            return Vector3.Zero;
        }
    }
}
