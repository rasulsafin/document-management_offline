using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class ObjectiveIssueConverter : IConverter<ObjectiveExternalDto, Issue>
    {
        private readonly Bim360ConnectionContext context;
        private readonly IConverter<ObjectiveStatus, Status> statusConverter;
        private readonly IssuesService issuesService;
        private readonly FoldersService foldersService;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly ItemsService itemsService;

        public ObjectiveIssueConverter(
            Bim360ConnectionContext context,
            IConverter<ObjectiveStatus, Status> statusConverter,
            IssuesService issuesService,
            FoldersService foldersService,
            ItemsSyncHelper itemsSyncHelper,
            ItemsService itemsService)
        {
            this.context = context;
            this.statusConverter = statusConverter;
            this.issuesService = issuesService;
            this.foldersService = foldersService;
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

            if (exist == null)
            {
                var issueType = GetIssueType(objective);
                (type, subtype) = (issueType.ID, issueType.Subtypes[0].ID);
                (targetUrn, startingVersion) = await GetTarget(objective, project);
            }
            else
            {
                type = exist.Attributes.NgIssueTypeID;
                subtype = exist.Attributes.NgIssueSubtypeID;
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
                    AssignedTo = GetDynamicField(objective.DynamicFields, nameof(Issue.Attributes.AssignedTo)),
                    CreatedAt = ConvertToNullable(objective.CreationDate),
                    DueDate = ConvertToNullable(objective.DueDate),
                    //// TODO: LocationDescription,
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

        private static string GetDynamicField(IEnumerable<DynamicFieldExternalDto> dynamicFields, string fieldName)
        {
            var field = dynamicFields?.FirstOrDefault(f => f.ExternalID == fieldName);
            return field?.Value;
        }

        private static void SetBimElements(ObjectiveExternalDto objective, Issue result)
        {
            if (objective.BimElements == null)
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

        private IssueType GetIssueType(ObjectiveExternalDto obj)
        {
            var types = context.Snapshot.ProjectEnumerable.First(x => x.Key == obj.ProjectExternalID).Value.IssueTypes;
            var dynamicFieldID =
                typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.NgIssueTypeID));
            var dynamicField = obj.DynamicFields.First(d => d.ExternalID == dynamicFieldID);
            var type = types.FirstOrDefault(x => x.Value.Title == dynamicField.Value).Value ?? types.First().Value;
            return type;
        }

        private ProjectSnapshot GetProjectSnapshot(ObjectiveExternalDto obj)
            => context.Snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID)
               .Value;

        private async Task<(string item, int? version)> GetTarget(ObjectiveExternalDto obj, ProjectSnapshot project)
        {
            async Task<Version> GetVersion(string itemID, long size)
            {
                var versions = (await itemsService.GetVersions(project.ID, itemID))
                   .OrderByDescending(x => x.Attributes.VersionNumber)
                   .ToArray();
                return versions.FirstOrDefault(x => x.Attributes.StorageSize == size) ?? versions.First();
            }

            if (obj.Location == null)
                return default;

            Item item;
            Version version;

            if (obj.Location.Item.ExternalID == null)
            {
                var snapshot = project.FindItemByName(obj.Location.Item.FileName);
                item = snapshot.Entity;
                var size = new FileInfo(obj.Location.Item.FullPath).Length;
                version = snapshot.Version.Attributes.StorageSize == size
                    ? snapshot.Version
                    : await GetVersion(item.ID, size);

                if (item == default && version == default)
                    (item, version) = await itemsSyncHelper.PostItem(project, obj.Location.Item);
            }
            else
            {
                (item, version) = await itemsService.GetAsync(project.ID, obj.Location.Item.ExternalID);
            }

            return (item?.ID, version?.Attributes.VersionNumber);
        }

        private async Task<Vector3> GetGlobalOffset(string containerID, string targetUrn)
        {
            if (!string.IsNullOrWhiteSpace(targetUrn))
            {
                var filter = new Filter(
                    typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.TargetUrn)),
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
