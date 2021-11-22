using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Synchronization.Models;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class ObjectiveIssueConverter : IConverter<ObjectiveExternalDto, Issue>
    {
        private readonly SnapshotGetter snapshot;
        private readonly IConverter<ObjectiveExternalDto, Status> statusConverter;
        private readonly IIssuesService issuesService;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly ItemsService itemsService;
        private readonly TypeSubtypeEnumCreator subtypeEnumCreator;
        private readonly RootCauseEnumCreator rootCauseEnumCreator;
        private readonly LocationEnumCreator locationEnumCreator;
        private readonly AssignToEnumCreator assignToEnumCreator;
        private readonly ConfigurationsHelper configurationsHelper;

        public ObjectiveIssueConverter(
            SnapshotGetter snapshot,
            IConverter<ObjectiveExternalDto, Status> statusConverter,
            IIssuesService issuesService,
            ItemsSyncHelper itemsSyncHelper,
            ItemsService itemsService,
            TypeSubtypeEnumCreator subtypeEnumCreator,
            RootCauseEnumCreator rootCauseEnumCreator,
            LocationEnumCreator locationEnumCreator,
            AssignToEnumCreator assignToEnumCreator,
            ConfigurationsHelper configurationsHelper)
        {
            this.snapshot = snapshot;
            this.statusConverter = statusConverter;
            this.issuesService = issuesService;
            this.itemsSyncHelper = itemsSyncHelper;
            this.itemsService = itemsService;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.locationEnumCreator = locationEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
            this.configurationsHelper = configurationsHelper;
        }

        public async Task<Issue> Convert(ObjectiveExternalDto objective)
        {
            Issue exist = null;
            var project = snapshot.GetProject(objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                exist = issueSnapshot.Entity;

            string type;
            string subtype;
            string targetUrn;
            string originalUrn = null;
            string[] permittedAttributes = null;
            Status[] permittedStatuses = null;
            int? startingVersion, originalStartingVersion = null;
            Vector3d? globalOffset = null;
            var typeSnapshot = GetIssueTypes(project, objective);
            var itemSnapshot = await GetTargetSnapshot(objective, project);
            var config = await configurationsHelper.GetModelConfig(
                objective.Location?.Item?.FileName,
                project,
                itemSnapshot);

            if (exist == null)
            {
                (type, subtype) = (typeSnapshot.ParentTypeID, typeSnapshot.SubtypeID);
                (targetUrn, startingVersion) = await GetTarget(objective, project, itemSnapshot);

                if (config != null)
                {
                    (targetUrn, originalUrn) = (config.RedirectTo.Urn, targetUrn);
                    (startingVersion, originalStartingVersion) = (config.RedirectTo.Version, startingVersion);
                }
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

            var assignToVariant = DynamicFieldUtilities.GetValue(
                assignToEnumCreator,
                project,
                objective,
                (ids, s) => ids.Contains(s.Entity),
                out _);
            var result = new Issue
            {
                ID = objective.ExternalID,
                Attributes = new Issue.IssueAttributes
                {
                    Title = objective.Title,
                    Description = objective.Description,
                    Status = await statusConverter.Convert(objective),
                    AssignedTo = assignToVariant?.Entity,
                    AssignedToType = assignToVariant?.Type,
                    CreatedAt = ConvertToNullable(objective.CreationDate),
                    DueDate = ConvertToNullable(objective.DueDate),
                    LocationDescription = GetDynamicField(objective.DynamicFields, x => x.LocationDescription),
                    RootCauseID =
                        DynamicFieldUtilities.GetValue(
                                rootCauseEnumCreator,
                                project,
                                objective,
                                (ids, s) => ids.Contains(s.Entity.ID),
                                out _)
                          ?.Entity.ID,
                    LbsLocation =
                        DynamicFieldUtilities.GetValue(
                                locationEnumCreator,
                                project,
                                objective,
                                (ids, s) => ids.Contains(s.Entity.ID),
                                out _)
                          ?.Entity.ID,
                    Answer = GetDynamicField(objective.DynamicFields, x => x.Answer),
                    PushpinAttributes =
                        await GetPushpinAttributes(objective.Location, project, targetUrn, globalOffset, config),
                    NgIssueTypeID = type,
                    NgIssueSubtypeID = subtype,
                    PermittedAttributes = permittedAttributes,
                    PermittedStatuses = permittedStatuses,
                    TargetUrn = targetUrn,
                    StartingVersion = startingVersion,
                },
            };

            SetOtherInfo(objective, result, config, originalUrn, originalStartingVersion);
            return result;
        }

        private static T? ConvertToNullable<T>(T value)
            where T : struct
            => EqualityComparer<T>.Default.Equals(value, default) ? null : value;

        private static string GetDynamicField(
            IEnumerable<DynamicFieldExternalDto> dynamicFields,
            Expression<Func<Issue.IssueAttributes, object>> property)
        {
            var fieldID = DataMemberUtilities.GetPath(property);
            var field = dynamicFields?.FirstOrDefault(f => f.ExternalID == fieldID);
            return field?.Value;
        }

        private static void SetOtherInfo(
            ObjectiveExternalDto objective,
            Issue result,
            IfcConfig config,
            string originalUrn,
            int? originalStartingVersion)
        {
            if (objective.Location == null)
                return;

            result.Attributes.PushpinAttributes ??= new Issue.PushpinAttributes();
            result.Attributes.PushpinAttributes.ViewerState ??= new Issue.ViewerState();
            var otherInfo = new OtherInfo();

            if (config != null &&
                originalUrn != null &&
                originalStartingVersion != null &&
                objective.Location?.Item?.ExternalID != null)
            {
                otherInfo.OriginalModelInfo = new LinkedInfo
                {
                    Urn = objective.Location?.Item?.ExternalID,
                    Version = originalStartingVersion.Value,
                    Offset = config.RedirectTo.Offset,
                };
            }

            result.Attributes.PushpinAttributes.ViewerState.OtherInfo = otherInfo;
        }

        private async Task<Issue.PushpinAttributes> GetPushpinAttributes(
            LocationExternalDto locationDto,
            ProjectSnapshot projectSnapshot,
            string targetUrn,
            Vector3d? globalOffset = default,
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
            var type = DynamicFieldUtilities.GetValue(
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

        private async Task<(string item, int? version)> GetTarget(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            ItemSnapshot itemSnapshot)
        {
            async Task<Forge.Models.DataManagement.Version> GetVersion(string itemID, long size)
            {
                if (itemID == null)
                    return null;

                var versions = (await itemsService.GetVersions(project.ID, itemID))
                   .OrderByDescending(x => x.Attributes.VersionNumber ?? 0)
                   .ToArray();
                return versions.FirstOrDefault(x => x.Attributes.StorageSize == size) ?? versions.First();
            }

            if (itemSnapshot != null)
            {
                Item item = itemSnapshot.Entity;
                var info = obj.Location == null ? null : new FileInfo(obj.Location.Item.FullPath);
                var size = (info?.Exists ?? false) ? info.Length : 0;
                Forge.Models.DataManagement.Version version = size == 0 || itemSnapshot.Version.Attributes.StorageSize == size
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
                    itemSnapshot = project.Items[posted.ID];
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

        private async Task<Vector3d> GetGlobalOffsetOrZeroVector(ProjectSnapshot projectSnapshot, string targetUrn)
        {
            if (string.IsNullOrWhiteSpace(targetUrn))
                return Vector3d.Zero;

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
            => GetGlobalOffsetOrZeroVector(issue) != Vector3d.Zero;

        private Vector3d GetGlobalOffsetOrZeroVector(Issue issue)
            => issue?.Attributes?.PushpinAttributes?.ViewerState?.GlobalOffset ?? Vector3d.Zero;
    }
}
