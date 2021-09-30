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
        private readonly IConverter<ObjectiveExternalDto, Status> statusConverter;
        private readonly IssuesService issuesService;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly ItemsService itemsService;
        private readonly TypeSubtypeEnumCreator subtypeEnumCreator;
        private readonly RootCauseEnumCreator rootCauseEnumCreator;
        private readonly AssignToEnumCreator assignToEnumCreator;
        private readonly IfcConfigUtilities ifcConfigUtilities;

        public ObjectiveIssueConverter(
            Bim360Snapshot snapshot,
            IConverter<ObjectiveExternalDto, Status> statusConverter,
            IssuesService issuesService,
            ItemsSyncHelper itemsSyncHelper,
            ItemsService itemsService,
            TypeSubtypeEnumCreator subtypeEnumCreator,
            RootCauseEnumCreator rootCauseEnumCreator,
            AssignToEnumCreator assignToEnumCreator,
            IfcConfigUtilities ifcConfigUtilities)
        {
            this.snapshot = snapshot;
            this.statusConverter = statusConverter;
            this.issuesService = issuesService;
            this.itemsSyncHelper = itemsSyncHelper;
            this.itemsService = itemsService;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
            this.ifcConfigUtilities = ifcConfigUtilities;
        }

        public async Task<Issue> Convert(ObjectiveExternalDto objective)
        {
            Issue exist = null;
            var project = snapshot.ProjectEnumerable.First(x => x.ID == objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                exist = issueSnapshot.Entity;

            string type;
            string subtype;
            string targetUrn;
            string originalUrn = null;
            string[] permittedAttributes = null;
            Status[] permittedStatuses = null;
            int? startingVersion, originalStartingVersion = null;
            Vector3? globalOffset = null;
            var typeSnapshot = GetIssueTypes(project, objective);
            var itemSnapshot = await GetTargetSnapshot(objective, project);
            var config = await ifcConfigUtilities.GetConfig(objective, project, itemSnapshot);

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

            var assignToVariant = GetValue(
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
                        GetValue(rootCauseEnumCreator, project, objective, (ids, s) => ids.Contains(s.Entity.ID), out _)
                          ?.Entity.ID,
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
            if (objective.BimElements is not { Count: > 0 } || objective.Location == null)
                return;

            result.Attributes.PushpinAttributes ??= new Issue.PushpinAttributes();
            result.Attributes.PushpinAttributes.ViewerState ??= new Issue.ViewerState();
            var otherInfo = new OtherInfo { BimElements = objective.BimElements };

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

        private async Task<(string item, int? version)> GetTarget(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            ItemSnapshot itemSnapshot)
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
