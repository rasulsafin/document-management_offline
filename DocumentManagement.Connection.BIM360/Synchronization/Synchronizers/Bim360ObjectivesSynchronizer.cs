using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
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
        private readonly IssuesService issuesService;
        private readonly ItemsService itemsService;
        private readonly Bim360ConnectionContext context;
        private readonly Dictionary<string, (Issue, ObjectiveExternalDto)> objectives =
            new Dictionary<string, (Issue, ObjectiveExternalDto)>();

        public Bim360ObjectivesSynchronizer(
            Bim360ConnectionContext context,
            ItemsSyncHelper itemsSyncHelper,
            FoldersSyncHelper folderSyncHelper,
            ProjectsHelper projectsHelper,
            HubsHelper hubsHelper,
            IssuesService issuesService,
            ItemsService itemsService)
        {
            this.context = context;
            this.itemsSyncHelper = itemsSyncHelper;
            this.folderSyncHelper = folderSyncHelper;
            this.projectsHelper = projectsHelper;
            this.hubsHelper = hubsHelper;
            this.issuesService = issuesService;
            this.itemsService = itemsService;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();
            var (containerId, hubId, projectId) = await GetContainerId(obj);
            if (containerId == null)
                return null;

            var types = await issuesService.GetIssueTypesAsync(containerId);
            var dynamicFieldID = typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.NgIssueTypeID));
            var dynamicField = obj.DynamicFields.First(d => d.ExternalID == dynamicFieldID);
            var type = types.FirstOrDefault(x => x.Title == dynamicField.Value) ?? types[0];
            issue.Attributes.NgIssueTypeID = type.ID;
            issue.Attributes.NgIssueSubtypeID = type.Subtypes[0].ID;

            var created = await issuesService.PostIssueAsync(containerId, issue);

            var parsedToDto = created.ToExternalDto(
                context.Projects.FirstOrDefault(x => x.Value.Item1.Relationships.IssuesContainer.Data.ID == containerId)
                   .Key,
                types.First(x => x.ID == created.Attributes.NgIssueTypeID).Title);
            parsedToDto.ProjectExternalID = projectId;

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, hubId, projectId, created, containerId);
                parsedToDto.Items = added?.Select(i => i.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();
            var (containerId, _, _) = await GetContainerId(obj);
            if (containerId == null)
                return null;

            issue = await issuesService.GetIssueAsync(containerId, issue.ID);

            if (!issue.Attributes.PermittedStatuses.Contains(Status.Void))
            {
                issue.Attributes.Status = Status.Open;
                issue = await issuesService.PatchIssueAsync(containerId, issue);
            }

            issue.Attributes.Status = Status.Void;
            issue = await issuesService.PatchIssueAsync(containerId, issue);

            var types = await issuesService.GetIssueTypesAsync(containerId);
            return issue.ToExternalDto(
                context.Projects.FirstOrDefault(x => x.Value.Item1.Relationships.IssuesContainer.Data.ID == containerId)
                   .Key,
                types.First(x => x.ID == issue.Attributes.NgIssueTypeID).Title);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var issue = obj.ToIssue();
            var (containerId, hubId, projectId) = await GetContainerId(obj);
            if (containerId == null)
                return null;

            var issueFromRemote = await issuesService.GetIssueAsync(containerId, issue.ID);
            issue.Attributes.PermittedAttributes = issueFromRemote.Attributes.PermittedAttributes;
            issue.Attributes.NgIssueTypeID = issueFromRemote.Attributes.NgIssueTypeID;
            issue.Attributes.NgIssueSubtypeID = issueFromRemote.Attributes.NgIssueSubtypeID;
            var updatedIssue = await issuesService.PatchIssueAsync(containerId, issue);
            var types = await issuesService.GetIssueTypesAsync(containerId);

            var parsedToDto = updatedIssue.ToExternalDto(
                context.Projects.FirstOrDefault(x => x.Value.Item1.Relationships.IssuesContainer.Data.ID == containerId)
                   .Key,
                types.First(x => x.ID == updatedIssue.Attributes.NgIssueTypeID).Title);

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, hubId, projectId, issueFromRemote, containerId);
                parsedToDto.Items = added?.Select(i => i.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            var ids = new List<string>();
            await context.UpdateProjects(false);
            objectives.Clear();

            foreach (var project in context.Projects)
            {
                var container = project.Value.Item1.Relationships.IssuesContainer.Data.ID;
                var statusKey = typeof(Issue.IssueAttributes)
                   .GetDataMemberName(nameof(Issue.IssueAttributes.Status));
                var statusFilter = new Filter(
                    statusKey,
                    Status.Draft.GetEnumMemberValue(),
                    Status.Answered.GetEnumMemberValue(),
                    Status.Closed.GetEnumMemberValue(),
                    Status.Open.GetEnumMemberValue());
                var updatedFilter = new Filter(FILTER_KEY_ISSUE_UPDATED_AFTER, date.ToString("O"));
                var filters = new[]
                    {
                        updatedFilter,
                        statusFilter,
                    };
                var issues = await issuesService.GetIssuesAsync(container, filters);

                foreach (var issue in issues.Where(issue => !ids.Contains(issue.ID)))
                {
                    var dto = await GetFullObjectiveExternalDto(issue, project.Key, container);
                    ids.Add(dto.ExternalID);
                    objectives.Add(issue.ID, (issue, dto));
                }
            }

            return ids;
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var result = new List<ObjectiveExternalDto>();
            await context.UpdateProjects(false);

            foreach (var id in ids)
            {
                if (objectives.TryGetValue(id, out var objective))
                {
                    result.Add(objective.Item2);
                }
                else
                {
                    foreach (var project in context.Projects)
                    {
                        Issue found = null;
                        var container = project.Value.Item1.Relationships.IssuesContainer.Data.ID;

                        try
                        {
                            found = await issuesService.GetIssueAsync(container, id);
                        }
                        catch
                        {
                        }

                        if (found != null)
                        {
                            result.Add(await GetFullObjectiveExternalDto(found, project.Key, container));
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private async Task<ObjectiveExternalDto> GetFullObjectiveExternalDto(
            Issue issue,
            string projectID,
            string container)
        {
            var types = await issuesService.GetIssueTypesAsync(container);
            var dto = issue.ToExternalDto(projectID, types.First(x => x.ID == issue.Attributes.NgIssueTypeID).Title);
            dto.Items ??= new List<ItemExternalDto>();

            var attachments = await issuesService.GetAttachmentsAsync(container, issue.ID);

            foreach (var attachment in attachments)
                dto.Items.Add((await itemsService.GetAsync(projectID, attachment.Attributes.Urn)).item.ToDto());
            return dto;
        }

        private async Task<IEnumerable<Item>> AddItems(
            ICollection<ItemExternalDto> items,
            string hubId,
            string projectId,
            Issue issue,
            string containerId)
        {
            if (!context.DefaultFolders.TryGetValue(projectId, out var folder))
                return null;

            var resultItems = new List<Item>();
            var existingItems = await folderSyncHelper.GetFolderItemsAsync(projectId, folder.ID);
            var attachment = await issuesService.GetAttachmentsAsync(containerId, issue.ID);

            foreach (var item in items)
            {
                // If item with the same name already exists add existing item
                var itemWithSameNameExists = existingItems.FirstOrDefault(i => i.Attributes.DisplayName.Equals(item.FileName, StringComparison.InvariantCultureIgnoreCase));
                if (itemWithSameNameExists != null)
                {
                    if (attachment.Any(x => x.Attributes.Name == item.FileName))
                    {
                        resultItems.Add(itemWithSameNameExists);
                        continue;
                    }

                    var attached = await AttachItem(itemWithSameNameExists, issue.ID, containerId);
                    if (attached)
                        resultItems.Add(itemWithSameNameExists);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.ExternalID))
                {
                    var posted = await itemsSyncHelper.PostItem(item, folder, projectId);
                    await Task.Delay(5000);
                    var attached = await AttachItem(posted, issue.ID, containerId);
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
                await issuesService.PostIssuesAttachmentsAsync(containerId, attachment);
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
