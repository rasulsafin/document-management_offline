using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    internal class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly IssuesService issuesService;
        private readonly ItemsService itemsService;
        private readonly ConverterAsync<ObjectiveExternalDto, Issue> convertToIssueAsync;
        private readonly ConverterAsync<IssueSnapshot, ObjectiveExternalDto> convertToDtoAsync;
        private readonly SnapshotFiller filler;
        private readonly FoldersService foldersService;
        private readonly Bim360ConnectionContext context;

        public Bim360ObjectivesSynchronizer(
            Bim360ConnectionContext context,
            ItemsSyncHelper itemsSyncHelper,
            IssuesService issuesService,
            ItemsService itemsService,
            ConverterAsync<ObjectiveExternalDto, Issue> convertToIssueAsync,
            ConverterAsync<IssueSnapshot, ObjectiveExternalDto> convertToDtoAsync,
            SnapshotFiller filler,
            FoldersService foldersService)
        {
            this.context = context;
            this.itemsSyncHelper = itemsSyncHelper;
            this.issuesService = issuesService;
            this.itemsService = itemsService;
            this.convertToIssueAsync = convertToIssueAsync;
            this.convertToDtoAsync = convertToDtoAsync;
            this.filler = filler;
            this.foldersService = foldersService;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var issue = await convertToIssueAsync(obj);
            var project = GetProjectSnapshot(obj);

            var created = await issuesService.PostIssueAsync(project.IssueContainer, issue);
            var snapshot = new IssueSnapshot(created, project);
            project.Issues.Add(created.ID, snapshot);
            var parsedToDto = await convertToDtoAsync(snapshot);

            if (obj.Items?.Any() ?? false)
            {
                snapshot.Items = new List<ItemSnapshot>();
                var added = await AddItems(obj.Items, project.ID, created, project.IssueContainer);
                snapshot.Items.AddRange(added.Select(x => new ItemSnapshot(x)));
                parsedToDto.Items = snapshot.Items.Select(i => i.Entity.ToDto()).ToList();
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

            return await convertToDtoAsync(snapshot);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var issue = await convertToIssueAsync(obj);
            var project = GetProjectSnapshot(obj);
            var snapshot = project.Issues[obj.ExternalID];

            snapshot.Entity = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            var parsedToDto = await convertToDtoAsync(snapshot);

            if (obj.Items?.Any() ?? false)
            {
                snapshot.Items ??= new List<ItemSnapshot>();
                var added = await AddItems(obj.Items, project.ID, snapshot.Entity, project.ID);
                snapshot.Items.AddRange(added.Select(x => new ItemSnapshot(x)));
                parsedToDto.Items = snapshot.Items.Select(i => i.Entity.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await filler.UpdateHubsIfNull();
            await filler.UpdateProjectsIfNull();
            await filler.UpdateIssuesIfNull(date);
            return context.Snapshot.IssueEnumerable.Where(x => x.Value.Entity.Attributes.UpdatedAt > date)
               .Select(x => x.Key)
               .ToList();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var result = new List<ObjectiveExternalDto>();

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
                            if (IssueUtilities.IsRemoved(received))
                                break;

                            found = new IssueSnapshot(received, project.Value);
                            break;
                        }
                    }
                }

                if (found == null)
                    continue;

                found.Items = new List<ItemSnapshot>();

                var attachments = await issuesService.GetAttachmentsAsync(
                    found.ProjectSnapshot.IssueContainer,
                    found.ID);

                foreach (var attachment in attachments)
                {
                    var (item, version) = await itemsService.GetAsync(found.ProjectSnapshot.ID, attachment.Attributes.Urn);

                    found.Items.Add(new ItemSnapshot(item) { Version = version });
                }

                result.Add(await convertToDtoAsync(found));

                if (!found.ProjectSnapshot.Issues.ContainsKey(found.Entity.ID))
                    found.ProjectSnapshot.Issues.Add(found.Entity.ID, found);
            }

            return result;
        }

        private async Task<IEnumerable<Item>> AddItems(
            ICollection<ItemExternalDto> items,
            string projectId,
            Issue issue,
            string containerId)
        {
            var folder = context.Snapshot.ProjectEnumerable.First(x => x.Key == projectId).Value.ProjectFilesFolder;

            var resultItems = new List<Item>();
            var fileTuples = await foldersService.SearchAsync(projectId, folder.ID);
            var existingItems = fileTuples.Select(t => t.item);
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
