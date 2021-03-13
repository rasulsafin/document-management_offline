using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    public class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly FoldersSyncHelper folderSyncHelper;
        private readonly ProjectsHelper projectsHelper;
        private readonly HubsHelper hubsHelper;
        private readonly Bim360ConnectionContext context;

        public Bim360ObjectivesSynchronizer(Bim360ConnectionContext context)
        {
            this.context = context;
            itemsSyncHelper = new ItemsSyncHelper(
                context.ItemsService,
                context.ProjectsService,
                context.ObjectsService,
                context.VersionsService);
            folderSyncHelper = new FoldersSyncHelper(context.FoldersService, context.ProjectsService);
            projectsHelper = new ProjectsHelper(context.ProjectsService);
            hubsHelper = new HubsHelper(context.HubsService);
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();
            var (containerId, hubId, projectId) = await GetContainerId(obj);
            if (containerId == null)
                return null;

            var created = await context.IssuesService.PostIssueAsync(containerId, issue);

            var parsedToDto = created.ToExternalDto(
                context.Projects.FirstOrDefault(x => x.Value.Item1.Relationships.IssuesContainer.Data.ID == containerId)
                   .Key);
            parsedToDto.ProjectExternalID = projectId;

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, hubId, projectId, created.ID, containerId);
                parsedToDto.Items = added?.Select(i => i.ToDto())?.ToList();
            }

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();
            var (containerId, _, _) = await GetContainerId(obj);
            if (containerId == null)
                return null;

            issue = await context.IssuesService.GetIssueAsync(containerId, issue.ID);
            issue.Attributes.Status = ISSUE_STATUS_CLOSED;
            var updatedIssue = await context.IssuesService.PatchIssueAsync(containerId, issue);

            return updatedIssue.ToExternalDto(context.Projects.FirstOrDefault(x => x.Value.Item1.Relationships.IssuesContainer.Data.ID == containerId)
               .Key);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();
            var (containerId, hubId, projectId) = await GetContainerId(obj);
            if (containerId == null)
                return null;

            issue.Attributes.PermittedAttributes
                = (await context.IssuesService.GetIssueAsync(containerId, issue.ID)).Attributes.PermittedAttributes;
            var updatedIssue = await context.IssuesService.PatchIssueAsync(containerId, issue);

            return updatedIssue.ToExternalDto(context.Projects.FirstOrDefault(x => x.Value.Item1.Relationships.IssuesContainer.Data.ID == containerId)
               .Key);
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            var objectives = new List<ObjectiveExternalDto>();
            await context.UpdateProjects(false);

            foreach (var project in context.Projects)
            {
                var issues = await context.IssuesService.GetIssuesAsync(project.Value.Item1.Relationships.IssuesContainer.Data.ID);

                foreach (var issue in issues)
                {
                    var dto = issue.ToExternalDto(project.Key);
                    dto.Items ??= new List<ItemExternalDto>();

                    var attachments = await context.IssuesService.GetAttachmentsAsync(
                        project.Value.Item1.Relationships.IssuesContainer.Data.ID,
                        issue.ID);

                    foreach (var attachment in attachments)
                        dto.Items.Add((await context.ItemsService.GetAsync(project.Value.Item1.ID, attachment.Attributes.Urn)).ToDto());

                    objectives.Add(dto);
                }
            }

            return objectives.Select(x => x.ExternalID).ToArray();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var objectives = new List<ObjectiveExternalDto>();
            await context.UpdateProjects(false);

            foreach (var project in context.Projects)
            {
                var issues = await context.IssuesService.GetIssuesAsync(project.Value.Item1.Relationships.IssuesContainer.Data.ID);

                foreach (var issue in issues)
                {
                    var dto = issue.ToExternalDto(project.Key);
                    dto.Items ??= new List<ItemExternalDto>();

                    var attachments = await context.IssuesService.GetAttachmentsAsync(
                        project.Value.Item1.Relationships.IssuesContainer.Data.ID,
                        issue.ID);

                    foreach (var attachment in attachments)
                        dto.Items.Add((await context.ItemsService.GetAsync(project.Value.Item1.ID, attachment.Attributes.Urn)).ToDto());

                    objectives.Add(dto);
                }
            }

            return objectives;
        }

        private async Task<IEnumerable<Item>> AddItems(
            ICollection<ItemExternalDto> items,
            string hubId,
            string projectId,
            string issueId,
            string containerId)
        {
            if (!context.DefaultFolders.TryGetValue(projectId, out var folder))
                return null;

            var resultItems = new List<Item>();
            var existingItems = await folderSyncHelper.GetFolderItemsAsync(projectId, folder.ID);

            foreach (var item in items)
            {
                // If item with the same name already exists add existing item
                var itemWithSameNameExists = existingItems.FirstOrDefault(i => i.Attributes.DisplayName.Equals(item.FileName, StringComparison.InvariantCultureIgnoreCase));
                if (itemWithSameNameExists != null)
                {
                    resultItems.Add(itemWithSameNameExists);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.ExternalID))
                {
                    var posted = await itemsSyncHelper.PostItem(item, folder, projectId);
                    var attached = await AttachItem(posted, issueId, containerId);
                    if (attached)
                        resultItems.Add(posted);
                }
            }

            return resultItems;
        }

        private async Task<bool> AttachItem(Item posted, string issueId, string containerId)
        {
            var attachment = new Attachment
            {
                Attributes = new Attachment.AttachmentAttributes
                {
                    Name = posted.Attributes.DisplayName,
                    IssueId = issueId,
                    Urn = posted.ID,
                },
            };

            try
            {
                await context.IssuesService.PostIssuesAttachmentsAsync(containerId, attachment);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private async Task<(string containerId, string hubId, string projectId)> GetContainerId(ObjectiveExternalDto obj)
        {
            var hub = await hubsHelper.GetDefaultHubAsync();
            var project = await projectsHelper.GetProjectAsync(hub.ID, p => p.ID == obj.ProjectExternalID);
            var containerId = project.Relationships?.IssuesContainer?.Data?.ID;

            return (containerId, hub.ID, project.ID);
        }
    }
}
