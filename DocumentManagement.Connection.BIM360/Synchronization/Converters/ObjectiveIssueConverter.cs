using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface;
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
        private readonly TypeSubtypeEnumCreator subtypeEnumCreator;
        private readonly RootCauseEnumCreator rootCauseEnumCreator;
        private readonly AssignToEnumCreator assignToEnumCreator;

        public ObjectiveIssueConverter(
            Bim360Snapshot snapshot,
            IConverter<ObjectiveStatus, Status> statusConverter,
            IssuesService issuesService,
            ItemsSyncHelper itemsSyncHelper,
            ItemsService itemsService,
            TypeSubtypeEnumCreator subtypeEnumCreator,
            RootCauseEnumCreator rootCauseEnumCreator,
            AssignToEnumCreator assignToEnumCreator)
        {
            this.snapshot = snapshot;
            this.statusConverter = statusConverter;
            this.issuesService = issuesService;
            this.itemsSyncHelper = itemsSyncHelper;
            this.itemsService = itemsService;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
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

            var assignToVariant = GetValue(
                assignToEnumCreator,
                project,
                objective,
                (ids, s) => ids.Contains(s.Entity.ID),
                out _);
            var result = new Issue
            {
                ID = objective.ExternalID,
                Attributes = new Issue.IssueAttributes
                {
                    Title = objective.Title,
                    Description = objective.Description,
                    Status = await statusConverter.Convert(objective.Status),
                    AssignedTo = assignToVariant?.Entity.ID,
                    AssignedToType = assignToVariant?.Entity.Type,
                    CreatedAt = ConvertToNullable(objective.CreationDate),
                    DueDate = ConvertToNullable(objective.DueDate),
                    LocationDescription = GetDynamicField(objective.DynamicFields, x => x.LocationDescription),
                    RootCauseID =
                        GetValue(rootCauseEnumCreator, project, objective, (ids, s) => ids.Contains(s.Entity.ID), out _)
                          ?.Entity.ID,
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
            var type = GetValue(
                subtypeEnumCreator,
                projectSnapshot,
                obj,
                (ids, s) => ids.Any(x => x.subtypeID == s.SubtypeID),
                out var deserializedIDs);

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

        private TSnapshot GetValue<T, TSnapshot, TID>(
            IEnumCreator<T, TSnapshot, TID> creator,
            ProjectSnapshot projectSnapshot,
            ObjectiveExternalDto obj,
            Func<IEnumerable<TID>, TSnapshot, bool> findPredicate,
            out IEnumerable<TID> deserializedIDs)
            where TSnapshot : AEnumVariantSnapshot<T>
        {
            deserializedIDs = null;
            var dynamicField = obj.DynamicFields.First(d => d.ExternalID == creator.EnumExternalID);

            if (creator.CanBeNull && dynamicField.Value == creator.NullID)
                return null;

            var ids = creator.DeserializeID(dynamicField.Value).ToArray();
            deserializedIDs = ids;
            return creator.GetSnapshots(projectSnapshot)
               .FirstOrDefault(x => findPredicate(ids, x));
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
                    var info = new FileInfo(obj.Location.Item.FullPath);
                    var size = info.Exists ? info.Length : 0;
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
