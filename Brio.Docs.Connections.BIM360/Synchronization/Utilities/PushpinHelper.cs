using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Synchronization.Models;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    internal class PushpinHelper
    {
        private readonly SnapshotGetter snapshot;
        private readonly IssuesService issuesService;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly ItemsService itemsService;
        private readonly ConfigurationsHelper configurationsHelper;

        public PushpinHelper(SnapshotGetter snapshot,
            IssuesService issuesService,
            ItemsSyncHelper itemsSyncHelper,
            ItemsService itemsService,
            ConfigurationsHelper configurationsHelper)
        {
            this.snapshot = snapshot;
            this.issuesService = issuesService;
            this.itemsSyncHelper = itemsSyncHelper;
            this.itemsService = itemsService;
            this.configurationsHelper = configurationsHelper;
        }

        public async Task<(Issue, LinkedInfo)> ConvertToPushpin(Issue issue, ObjectiveExternalDto objective)
        {
            Issue exist = null;
            var project = snapshot.GetProject(objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                exist = issueSnapshot.Entity;

            string targetUrn;
            string originalUrn = null;
            int? startingVersion, originalStartingVersion = null;
            Vector3d? globalOffset = null;
            var itemSnapshot = await GetTargetSnapshot(objective, project);
            var config = await configurationsHelper.GetModelConfig(
                objective.Location?.Item?.FileName,
                project,
                itemSnapshot);

            if (exist == null)
            {
                (targetUrn, startingVersion) = await GetTarget(objective, project, itemSnapshot);

                if (config != null)
                {
                    (targetUrn, originalUrn) = (config.RedirectTo.Urn, targetUrn);
                    (startingVersion, originalStartingVersion) = (config.RedirectTo.Version, startingVersion);
                }
            }
            else
            {
                targetUrn = exist.Attributes.TargetUrn;
                startingVersion = exist.Attributes.StartingVersion;
                globalOffset = exist.Attributes.PushpinAttributes?.ViewerState?.GlobalOffset;
            }

            var result = issue;
            result.Attributes.StartingVersion = startingVersion;
            result.Attributes.TargetUrn = targetUrn;
            result.Attributes.PushpinAttributes = await GetPushpinAttributes(objective.Location, project, targetUrn, globalOffset, config);

            var linkedInfo = SetOtherInfo(objective, result, config, originalUrn, originalStartingVersion);
            return (result, linkedInfo);
        }

        private static LinkedInfo SetOtherInfo(
            ObjectiveExternalDto objective,
            Issue result,
            IfcConfig config,
            string originalUrn,
            int? originalStartingVersion)
        {
            if (objective.Location == null)
                return null;

            result.Attributes.PushpinAttributes ??= new Issue.PushpinAttributes();
            result.Attributes.PushpinAttributes.ViewerState ??= new Issue.ViewerState();

            if (config != null &&
                originalUrn != null &&
                originalStartingVersion != null &&
                objective.Location?.Item?.ExternalID != null)
            {
                var linkedInfo = new LinkedInfo
                {
                    Urn = objective.Location?.Item?.ExternalID,
                    Version = originalStartingVersion.Value,
                    Offset = config.RedirectTo.Offset,
                };

                return linkedInfo;
            }

            return null;
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
