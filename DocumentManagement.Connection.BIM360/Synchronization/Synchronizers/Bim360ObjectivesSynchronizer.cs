using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    internal class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly IssuesService issuesService;
        private readonly ItemsService itemsService;
        private readonly Authenticator authenticator;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly Bim360ConnectionContext context;
        private readonly IBim360SnapshotFiller filler;
        private readonly IConverter<ObjectiveExternalDto, Issue> converterToIssue;
        private readonly IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto;

        public Bim360ObjectivesSynchronizer(
            Bim360ConnectionContext context,
            IssuesService issuesService,
            ItemsService itemsService,
            Authenticator authenticator,
            ItemsSyncHelper itemsSyncHelper,
            IBim360SnapshotFiller filler,
            IConverter<ObjectiveExternalDto, Issue> converterToIssue,
            IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto)
        {
            this.context = context;
            this.itemsSyncHelper = itemsSyncHelper;
            this.issuesService = issuesService;
            this.itemsService = itemsService;
            this.converterToIssue = converterToIssue;
            this.converterToDto = converterToDto;
            this.filler = filler;
            this.authenticator = authenticator;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var issue = await converterToIssue.Convert(obj);
            var project = GetProjectSnapshot(obj);
            var snapshot = new IssueSnapshot(issue, project);
            var created = await issuesService.PostIssueAsync(project.IssueContainer, issue);
            snapshot.Entity = created;
            project.Issues.Add(created.ID, snapshot);
            var parsedToDto = await converterToDto.Convert(snapshot);

            if (obj.Items?.Any() ?? false)
            {
                snapshot.Items = new List<ItemSnapshot>();
                var added = await AddItems(obj.Items, project, created);
                snapshot.Items.AddRange(added.Select(x => new ItemSnapshot(x)));
                parsedToDto.Items = snapshot.Items.Select(i => i.Entity.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var issue = await converterToIssue.Convert(obj);
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

            return await converterToDto.Convert(snapshot);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var project = GetProjectSnapshot(obj);
            var snapshot = project.Issues[obj.ExternalID];
            snapshot.Entity = await converterToIssue.Convert(obj);
            snapshot.Entity = await issuesService.PatchIssueAsync(project.IssueContainer, snapshot.Entity);
            var parsedToDto = await converterToDto.Convert(snapshot);

            if (obj.Items?.Any() ?? false)
            {
                snapshot.Items ??= new List<ItemSnapshot>();
                var added = await AddItems(obj.Items, project, snapshot.Entity);
                snapshot.Items.AddRange(added.Select(x => new ItemSnapshot(x)));
                parsedToDto.Items = snapshot.Items.Select(i => i.Entity.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            await filler.UpdateHubsIfNull();
            await filler.UpdateProjectsIfNull();
            await filler.UpdateIssuesIfNull(date);
            await filler.UpdateIssueTypes();
            return context.Snapshot.IssueEnumerable.Where(x => x.Value.Entity.Attributes.UpdatedAt > date)
               .Select(x => x.Key)
               .ToList();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var result = new List<ObjectiveExternalDto>();

            foreach (var id in ids)
            {
                try
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
                                received = await issuesService.GetIssueAsync(container, id);
                            }
                            catch
                            {
                            }

                            if (received != null)
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
                        try
                        {
                            var (item, version) = await itemsService.GetAsync(
                                found.ProjectSnapshot.ID,
                                attachment.Attributes.Urn);

                            found.Items.Add(new ItemSnapshot(item) { Version = version });
                        }
                        catch (Exception e)
                        {
                        }
                    }

                    result.Add(await converterToDto.Convert(found));

                    if (!found.ProjectSnapshot.Issues.ContainsKey(found.Entity.ID))
                        found.ProjectSnapshot.Issues.Add(found.Entity.ID, found);
                }
                catch (Exception e)
                {
                }
            }

            return result;
        }

        private async Task<IEnumerable<Item>> AddItems(
            ICollection<ItemExternalDto> items,
            ProjectSnapshot project,
            Issue issue)
        {
            var resultItems = new List<Item>();
            var attachment = await issuesService.GetAttachmentsAsync(project.IssueContainer, issue.ID);

            foreach (var item in items)
            {
                // If item with the same name already exists add existing item
                Item itemWithSameNameExists = project.FindItemByName(item.FileName)?.Entity;

                if (itemWithSameNameExists != null)
                {
                    if (attachment.Any(x => x.Attributes.Name == item.FileName))
                    {
                        resultItems.Add(itemWithSameNameExists);
                        continue;
                    }

                    var attached = await AttachItem(itemWithSameNameExists, issue.ID, project.IssueContainer);
                    if (attached)
                        resultItems.Add(itemWithSameNameExists);
                }
                else
                {
                    var uploadedItem = (await itemsSyncHelper.PostItem(project, item)).item;

                    if (uploadedItem == default)
                        continue;
                    await Task.Delay(5000);
                    var attached = await AttachItem(uploadedItem, issue.ID, project.IssueContainer);
                    if (attached)
                        resultItems.Add(uploadedItem);
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
