using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters;
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
        private readonly IssuesService issuesService;
        private readonly ItemsService itemsService;
        private readonly ConverterAsync<ObjectiveExternalDto, Issue> convertToIssueAsync;
        private readonly ConverterAsync<Issue, ObjectiveExternalDto> convertToDtoAsync;
        private readonly ConverterAsync<IssueType, DynamicFieldExternalDto> convertTypeAsync;
        private readonly Bim360ConnectionContext context;
        private readonly Dictionary<string, (Issue, ObjectiveExternalDto)> objectives =
            new Dictionary<string, (Issue, ObjectiveExternalDto)>();

        public Bim360ObjectivesSynchronizer(
            Bim360ConnectionContext context,
            ItemsSyncHelper itemsSyncHelper,
            FoldersSyncHelper folderSyncHelper,
            IssuesService issuesService,
            ItemsService itemsService,
            ConverterAsync<ObjectiveExternalDto, Issue> convertToIssueAsync,
            ConverterAsync<Issue, ObjectiveExternalDto> convertToDtoAsync,
            ConverterAsync<IssueType, DynamicFieldExternalDto> convertTypeAsync)
        {
            this.context = context;
            this.itemsSyncHelper = itemsSyncHelper;
            this.folderSyncHelper = folderSyncHelper;
            this.issuesService = issuesService;
            this.itemsService = itemsService;
            this.convertToIssueAsync = convertToIssueAsync;
            this.convertToDtoAsync = convertToDtoAsync;
            this.convertTypeAsync = convertTypeAsync;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var issue = await convertToIssueAsync(obj);
            var (containerId, projectId) = GetContainerId(obj);
            if (containerId == null)
                return null;

            var created = await issuesService.PostIssueAsync(containerId, issue);
            var types = await issuesService.GetIssueTypesAsync(containerId);
            var parsedToDto = await ParseToDto(created, types, projectId);

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, projectId, created, containerId);
                parsedToDto.Items = added?.Select(i => i.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            var issue = await convertToIssueAsync(obj);
            var (containerId, projectId) = GetContainerId(obj);
            if (containerId == null)
                return null;

            if (!issue.Attributes.PermittedStatuses.Contains(Status.Void))
            {
                issue.Attributes.Status = Status.Open;
                issue = await issuesService.PatchIssueAsync(containerId, issue);
            }

            issue.Attributes.Status = Status.Void;
            issue = await issuesService.PatchIssueAsync(containerId, issue);

            var types = await issuesService.GetIssueTypesAsync(containerId);
            return await ParseToDto(issue, types, projectId);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var issue = await convertToIssueAsync(obj);
            var (containerId, projectId) = GetContainerId(obj);
            if (containerId == null)
                return null;

            var updatedIssue = await issuesService.PatchIssueAsync(containerId, issue);
            var types = await issuesService.GetIssueTypesAsync(containerId);

            var parsedToDto = await ParseToDto(updatedIssue, types, projectId);

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, projectId, updatedIssue, containerId);
                parsedToDto.Items = added?.Select(i => i.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            var ids = new List<string>();
            await context.UpdateProjects(false);
            objectives.Clear();

            foreach (var project in context.Snapshot.ProjectEnumerable)
            {
                var container = project.Value.IssueContainer;
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
                    foreach (var project in context.Snapshot.ProjectEnumerable)
                    {
                        Issue found = null;
                        var container = project.Value.IssueContainer;

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

        private async Task<ObjectiveExternalDto> ParseToDto(Issue issue, IEnumerable<IssueType> types, string projectId)
        {
            var parsedToDto = await convertToDtoAsync(issue);
            var typeField = await convertTypeAsync(types.First(x => x.ID == issue.Attributes.NgIssueTypeID));
            parsedToDto.DynamicFields.Add(typeField);
            parsedToDto.ProjectExternalID = projectId;
            return parsedToDto;
        }

        private async Task<ObjectiveExternalDto> GetFullObjectiveExternalDto(
            Issue issue,
            string projectID,
            string container)
        {
            var types = await issuesService.GetIssueTypesAsync(container);
            var dto = await ParseToDto(issue, types, projectID);
            dto.Items ??= new List<ItemExternalDto>();

            var attachments = await issuesService.GetAttachmentsAsync(container, issue.ID);

            foreach (var attachment in attachments)
                dto.Items.Add((await itemsService.GetAsync(projectID, attachment.Attributes.Urn)).item.ToDto());
            return dto;
        }

        private async Task<IEnumerable<Item>> AddItems(
            ICollection<ItemExternalDto> items,
            string projectId,
            Issue issue,
            string containerId)
        {
            var folder = context.Snapshot.ProjectEnumerable.First(x => x.Key == projectId).Value.ProjectFilesFolder;

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

        private (string containerId, string projectId) GetContainerId(ObjectiveExternalDto obj)
        {
            var project = context.Snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID);
            var containerId = project.Value.IssueContainer;

            return (containerId, project.Key);
        }
    }
}
