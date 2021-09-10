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
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Models;
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
        private readonly IfcConfigUtilities ifcConfigUtilities;

        public ObjectiveIssueConverter(
            Bim360Snapshot snapshot,
            IConverter<ObjectiveStatus, Status> statusConverter,
            IssuesService issuesService,
            ItemsSyncHelper itemsSyncHelper,
            ItemsService itemsService,
            IfcConfigUtilities ifcConfigUtilities)
        {
            this.snapshot = snapshot;
            this.statusConverter = statusConverter;
            this.issuesService = issuesService;
            this.itemsSyncHelper = itemsSyncHelper;
            this.itemsService = itemsService;
            this.ifcConfigUtilities = ifcConfigUtilities;
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
            var itemSnapshot = await GetTargetSnapshot(objective, project);
            var config = await ifcConfigUtilities.GetConfig(objective, project, itemSnapshot);

            if (exist == null)
            {
                (type, subtype) = (typeSnapshot.ParentTypeID, typeSnapshot.SubtypeID);
                (targetUrn, startingVersion) = await GetTarget(objective, project, itemSnapshot, config);
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
                    ////AssignedTo = GetDynamicField(objective.DynamicFields, x => x.AssignedTo),
                    CreatedAt = ConvertToNullable(objective.CreationDate),
                    DueDate = ConvertToNullable(objective.DueDate),
                    LocationDescription = GetDynamicField(objective.DynamicFields, x => x.LocationDescription),
                    RootCauseID = GetRootCause(project, objective)?.Entity.ID,
                    Answer = GetDynamicField(objective.DynamicFields, x => x.Answer),
                    PushpinAttributes = await GetPushpinAttributes(objective.Location, project, targetUrn, globalOffset, config),
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

        private static T? ConvertToNullable<T>(T dateTime)
            where T : struct
            => Equals(dateTime, default(T)) ? null : dateTime;

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
            => new OtherInfo
            {
                BimElements = objective.BimElements,
                OriginalTargetUrn = objective.Location?.Item?.ExternalID,
            };

        private async Task<Issue.PushpinAttributes> GetPushpinAttributes(
            LocationExternalDto locationDto,
            ProjectSnapshot projectSnapshot,
            string targetUrn,
            Vector3? globalOffset = default,
            IfcConfig config = null)
        {
            if (locationDto == null)
                return null;

            var offset = globalOffset ?? (await GetGlobalOffsetOrZeroVector(projectSnapshot, targetUrn));
            var location = locationDto.Location.ToVector();
            var camera = locationDto.CameraPosition.ToVector();

            if (config != null)
            {
                location -= config.RedirectTo.Offset;
                camera -= config.RedirectTo.Offset;
            }

            var target = location.ToFeet().ToXZY();
            var eye = camera.ToFeet().ToXZY();

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
            var creator = new TypeSubtypeEnumCreator();
            var dynamicField = obj.DynamicFields.First(d => d.ExternalID == creator.EnumExternalID);
            var deserializedIDs = creator.DeserializeID(dynamicField.Value).ToArray();
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

        private RootCauseSnapshot GetRootCause(ProjectSnapshot projectSnapshot, ObjectiveExternalDto obj)
        {
            var creator = new RootCauseEnumCreator();
            var dynamicField = obj.DynamicFields.First(d => d.ExternalID == creator.EnumExternalID);

            if (dynamicField.Value == creator.NullID)
                return null;

            var deserializedIDs = creator.DeserializeID(dynamicField.Value).ToArray();
            var rootCause = projectSnapshot.RootCauses
               .FirstOrDefault(x => deserializedIDs.Contains(x.Value.Entity.ID))
               .Value;
            return rootCause;
        }

        private ProjectSnapshot GetProjectSnapshot(ObjectiveExternalDto obj)
            => snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID)
               .Value;

        private async Task<(string item, int? version)> GetTarget(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            ItemSnapshot itemSnapshot,
            IfcConfig ifcConfig)
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

            if (ifcConfig != null)
                return (ifcConfig.RedirectTo.Urn, ifcConfig.RedirectTo.Version);

            if (itemSnapshot != null)
            {
                Item item = itemSnapshot.Entity;
                var info = obj.Location == null ? null : new FileInfo(obj.Location.Item.FullPath);
                var size = (info?.Exists ?? false) ? info.Length : 0;
                Version version = size == 0 || itemSnapshot.Version.Attributes.StorageSize == size
                    ? itemSnapshot.Version
                    : await GetVersion(item?.ID, size);
                return (item?.ID, version?.Attributes.VersionNumber);
            }

            return default;
        }

        private async Task<ItemSnapshot> GetTargetSnapshot(ObjectiveExternalDto obj, ProjectSnapshot project)
        {
            if (obj.Location != null)
            {
                if (!project.Items.TryGetValue(obj.Location.Item.ExternalID, out var itemSnapshot))
                    itemSnapshot = project.FindItemByName(obj.Location.Item.FileName);

                if (itemSnapshot == null)
                {
                    var posted = await itemsSyncHelper.PostItem(project, obj.Location.Item);
                    itemSnapshot = new ItemSnapshot(posted.item) { Version = posted.version };
                    project.Items.Add(posted.item.ID, itemSnapshot);
                }

                return itemSnapshot;
            }

            if (obj.BimElements is { Count: > 0 })
            {
                return obj.BimElements.GroupBy(x => x.ParentName, (name, elements) => (name, count: elements.Count()))
                       .OrderByDescending(x => x.count)
                       .Select(x => x.name)
                       .Select(file => project.FindItemByName(file, true))
                       .FirstOrDefault(itemSnapshot => itemSnapshot != null);
            }

            return default;
        }

        private async Task<Vector3> GetGlobalOffsetOrZeroVector(ProjectSnapshot projectSnapshot, string targetUrn)
        {
            if (string.IsNullOrWhiteSpace(targetUrn))
                return Vector3.Zero;

            var found = projectSnapshot.Issues.Values
               .Select(issueSnapshot => issueSnapshot.Entity)
               .Where(issue => issue.Attributes.TargetUrn == targetUrn)
               .FirstOrDefault(IsNotZeroOffset);

            if (found == null)
            {
                var filter = new Filter(
                    DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.TargetUrn),
                    targetUrn);
                var issuesOnTarget = await issuesService.GetIssuesAsync(
                    projectSnapshot.IssueContainer,
                    new[] { filter });
                found = issuesOnTarget.FirstOrDefault(IsNotZeroOffset);
            }

            return GetGlobalOffsetOrZeroVector(found);
        }

        private bool IsNotZeroOffset(Issue issue)
            => GetGlobalOffsetOrZeroVector(issue) != Vector3.Zero;

        private Vector3 GetGlobalOffsetOrZeroVector(Issue issue)
            => issue?.Attributes?.PushpinAttributes?.ViewerState?.GlobalOffset ?? Vector3.Zero;
    }
}
