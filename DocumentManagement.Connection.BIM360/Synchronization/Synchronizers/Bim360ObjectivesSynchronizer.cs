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
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    public class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private static readonly Status REMOVED_STATUS = Status.Void;

        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly FoldersSyncHelper folderSyncHelper;
        private readonly IssuesService issuesService;
        private readonly ConverterAsync<ObjectiveExternalDto, Issue> convertToIssueAsync;
        private readonly ConverterAsync<IssueSnapshot, ObjectiveExternalDto> convertToDtoAsync;
        private readonly Bim360ConnectionContext context;

        public Bim360ObjectivesSynchronizer(
            Bim360ConnectionContext context,
            ItemsSyncHelper itemsSyncHelper,
            FoldersSyncHelper folderSyncHelper,
            IssuesService issuesService,
            ConverterAsync<ObjectiveExternalDto, Issue> convertToIssueAsync,
            ConverterAsync<IssueSnapshot, ObjectiveExternalDto> convertToDtoAsync)
        {
            this.context = context;
            this.itemsSyncHelper = itemsSyncHelper;
            this.folderSyncHelper = folderSyncHelper;
            this.issuesService = issuesService;
            this.convertToIssueAsync = convertToIssueAsync;
            this.convertToDtoAsync = convertToDtoAsync;
        }

        public static IEnumerable<Status> GetStatusesExceptRemoved()
            => Enum.GetValues(typeof(Status)).Cast<Status>().Where(x => x != REMOVED_STATUS);

        public static bool IsRemoved(Issue issue)
            => issue.Attributes.Status == REMOVED_STATUS;

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var issue = await convertToIssueAsync(obj);
            var project = GetProjectSnapshot(obj);

            var created = await issuesService.PostIssueAsync(project.IssueContainer, issue);
            var snapshot = new IssueSnapshot(created, project);
            project.Issues.Add(created.ID, snapshot);
            var parsedToDto = await ParseToDto(snapshot);

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, project.ID, created, project.IssueContainer);
                parsedToDto.Items = added?.Select(i => i.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            var issue = await convertToIssueAsync(obj);
            var project = GetProjectSnapshot(obj);
            var snapshot = project.Issues[obj.ExternalID];

            if (!issue.Attributes.PermittedStatuses.Contains(Status.Void))
            {
                issue.Attributes.Status = Status.Open;
                issue = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            }

            issue.Attributes.Status = Status.Void;
            snapshot.Entity = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            project.Issues.Remove(snapshot.ID);

            return await ParseToDto(snapshot);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var issue = await convertToIssueAsync(obj);
            var project = GetProjectSnapshot(obj);
            var snapshot = project.Issues[obj.ExternalID];

            snapshot.Entity = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            var parsedToDto = await ParseToDto(snapshot);

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, project.ID, snapshot.Entity, project.ID);
                parsedToDto.Items = added?.Select(i => i.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            var ids = new List<string>();
            await context.UpdateProjects(false);

            foreach (var project in context.Snapshot.ProjectEnumerable)
            {
                project.Value.Issues.Clear();
                var container = project.Value.IssueContainer;
                var statusKey = typeof(Issue.IssueAttributes)
                   .GetDataMemberName(nameof(Issue.IssueAttributes.Status));
                var statusFilter = new Filter(
                    statusKey,
                    GetStatusesExceptRemoved().Select(x => x.GetEnumMemberValue()).ToArray());
                var updatedFilter = new Filter(FILTER_KEY_ISSUE_UPDATED_AFTER, date.ToString("O"));
                var filters = new[]
                    {
                        updatedFilter,
                        statusFilter,
                    };
                var issues = await issuesService.GetIssuesAsync(container, filters);

                foreach (var issue in issues.Where(issue => !ids.Contains(issue.ID)))
                {
                    var snapshot = new IssueSnapshot(issue, project.Value);
                    project.Value.Issues.Add(issue.ID, snapshot);
                    var dto = await ParseToDto(snapshot);
                    ids.Add(dto.ExternalID);
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
                var found = context.Snapshot.IssueEnumerable.FirstOrDefault(x => x.Key == id).Value;

                if (found == null)
                {
                    foreach (var project in context.Snapshot.ProjectEnumerable)
                    {
                        var container = project.Value.IssueContainer;
                        Issue received = null;

                        try
                        {
                            received  = await issuesService.GetIssueAsync(container, id);
                        }
                        catch
                        {
                        }

                        if (received  != null)
                        {
                            if (IsRemoved(received))
                                break;

                            found = new IssueSnapshot(received, project.Value);
                            result.Add(await ParseToDto(found));
                            project.Value.Issues.Add(found.Entity.ID, found);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private async Task<ObjectiveExternalDto> ParseToDto(IssueSnapshot issue)
            => await convertToDtoAsync(issue);

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

        private ProjectSnapshot GetProjectSnapshot(ObjectiveExternalDto obj)
            => context.Snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID)
               .Value;
    }
}
