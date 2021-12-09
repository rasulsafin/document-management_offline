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
        private readonly ItemsService itemsService;
        private readonly ConfigurationsHelper configurationsHelper;

        public PushpinHelper(SnapshotGetter snapshot,
            IssuesService issuesService,
            ItemsService itemsService,
            ConfigurationsHelper configurationsHelper)
        {
            this.snapshot = snapshot;
            this.issuesService = issuesService;
            this.itemsService = itemsService;
            this.configurationsHelper = configurationsHelper;
        }

        public async Task<(Issue, LinkedInfo)> LinkToModel(Issue issueToChange, ObjectiveExternalDto objective, ItemSnapshot target)
        {
            Issue exist = null;
            var project = snapshot.GetProject(objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                exist = issueSnapshot.Entity;

            string targetUrn;
            string originalUrn = null;
            int? startingVersion, originalStartingVersion = null;
            Vector3d? globalOffset = null;
            target.Config ??= await configurationsHelper.GetModelConfig(
                objective.Location?.Item?.FileName,
                project,
                target);

            if (exist == null)
            {
                (targetUrn, startingVersion) = await GetTargetUrnAndVersion(objective, project, target);

                if (target.Config?.RedirectTo != null)
                {
                    (targetUrn, originalUrn) = (target.Config.RedirectTo.Urn, targetUrn);
                    (startingVersion, originalStartingVersion) = (target.Config.RedirectTo.Version, startingVersion);
                }
            }
            else
            {
                targetUrn = exist.Attributes.TargetUrn;
                startingVersion = exist.Attributes.StartingVersion;
                globalOffset = exist.Attributes.PushpinAttributes?.ViewerState?.GlobalOffset;
            }

            var result = issueToChange;
            result.Attributes.StartingVersion = startingVersion;
            result.Attributes.TargetUrn = targetUrn;
            result.Attributes.PushpinAttributes = await GetPushpinAttributes(
                objective.Location,
                project,
                targetUrn,
                globalOffset,
                target.Config?.RedirectTo);

            var linkedInfo = GetLinkedResultInfo(
                objective,
                result,
                target.Config,
                originalUrn,
                originalStartingVersion);
            return (result, linkedInfo);
        }

        private static LinkedInfo GetLinkedResultInfo(
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
            LinkedInfo redirectInfo = null)
        {
            if (locationDto == null)
                return null;

            var offset = globalOffset ?? (await GetGlobalOffsetOrZeroVector(projectSnapshot, targetUrn));
            var location = locationDto.Location.ToVector();
            var camera = locationDto.CameraPosition.ToVector();

            if (redirectInfo != null)
            {
                location -= redirectInfo.Offset;
                camera -= redirectInfo.Offset;
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

        private async Task<(string item, int? version)> GetTargetUrnAndVersion(
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

        private async Task<Vector3d> GetGlobalOffsetOrZeroVector(ProjectSnapshot projectSnapshot, string targetUrn)
        {
            if (string.IsNullOrWhiteSpace(targetUrn))
                return Vector3d.Zero;

            var targetSnapshot = projectSnapshot.Items.TryGetValue(targetUrn, out var value) ? value : null;
            if (targetSnapshot?.GlobalOffset != null)
                return targetSnapshot.GlobalOffset.Value;

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

            var vector = GetGlobalOffsetOrZeroVector(found);

            if (vector != Vector3d.Zero && targetSnapshot != null)
                targetSnapshot.GlobalOffset = vector;

            return vector;
        }

        private bool IsNotZeroOffset(Issue issue)
            => GetGlobalOffsetOrZeroVector(issue) != Vector3d.Zero;

        private Vector3d GetGlobalOffsetOrZeroVector(Issue issue)
            => issue?.Attributes?.PushpinAttributes?.ViewerState?.GlobalOffset ?? Vector3d.Zero;
    }
}
