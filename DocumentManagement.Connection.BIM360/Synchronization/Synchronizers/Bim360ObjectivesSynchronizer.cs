using Brio.Docs.Connection.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connection.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connection.Bim360.Forge.Services;
using Brio.Docs.Connection.Bim360.Forge.Utils;
using Brio.Docs.Connection.Bim360.Synchronization.Utilities;
using Brio.Docs.Connection.Bim360.Utilities.Snapshot;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Connection.Bim360.Synchronization.Extensions;

namespace Brio.Docs.Connection.Bim360.Synchronizers
{
    internal class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly Bim360Snapshot snapshot;
        private readonly IssuesService issuesService;
        private readonly Authenticator authenticator;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly SnapshotFiller filler;
        private readonly IConverter<ObjectiveExternalDto, Issue> converterToIssue;
        private readonly IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto;

        public Bim360ObjectivesSynchronizer(
            Bim360Snapshot snapshot,
            IssuesService issuesService,
            Authenticator authenticator,
            ItemsSyncHelper itemsSyncHelper,
            SnapshotFiller filler,
            IConverter<ObjectiveExternalDto, Issue> converterToIssue,
            IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto)
        {
            this.snapshot = snapshot;
            this.itemsSyncHelper = itemsSyncHelper;
            this.issuesService = issuesService;
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
            var issueSnapshot = new IssueSnapshot(issue, project);
            var created = await issuesService.PostIssueAsync(project.IssueContainer, issue);
            issueSnapshot.Entity = created;
            project.Issues.Add(created.ID, issueSnapshot);
            var parsedToDto = await converterToDto.Convert(issueSnapshot);

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, project, created);
                issueSnapshot.Items = added.ToDictionary(x => x.ID, x => new ItemSnapshot(x));
                parsedToDto.Items = issueSnapshot.Items.Values.Select(i => i.Entity.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var issue = await converterToIssue.Convert(obj);
            var project = GetProjectSnapshot(obj);
            var issueSnapshot = project.Issues[obj.ExternalID];

            if (!issue.Attributes.PermittedStatuses.Contains(Status.Void))
            {
                issue.Attributes.Status = Status.Open;
                issue = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            }

            issue.Attributes.Status = Status.Void;
            issueSnapshot.Entity = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            project.Issues.Remove(issueSnapshot.ID);

            return await converterToDto.Convert(issueSnapshot);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            var project = GetProjectSnapshot(obj);
            var issueSnapshot = project.Issues[obj.ExternalID];
            issueSnapshot.Entity = await converterToIssue.Convert(obj);
            issueSnapshot.Entity = await issuesService.PatchIssueAsync(project.IssueContainer, issueSnapshot.Entity);
            var parsedToDto = await converterToDto.Convert(issueSnapshot);

            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, project, issueSnapshot.Entity);
                issueSnapshot.Items = added.ToDictionary(x => x.ID, x => new ItemSnapshot(x));
                parsedToDto.Items = issueSnapshot.Items.Values.Select(i => i.Entity.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            await filler.UpdateIssuesIfNull(date);
            await filler.UpdateIssueTypes();
            await filler.UpdateRootCauses();
            await filler.UpdateAssignTo();
            return snapshot.IssueEnumerable.Where(x => x.Entity.Attributes.UpdatedAt > date)
               .Select(x => x.ID)
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
                    var found = snapshot.IssueEnumerable.FirstOrDefault(x => x.ID == id);

                    if (found == null)
                    {
                        foreach (var project in snapshot.ProjectEnumerable)
                        {
                            var container = project.IssueContainer;
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

                                found = new IssueSnapshot(received, project)
                                {
                                    Items = new Dictionary<string, ItemSnapshot>(),
                                };

                                var attachments = await issuesService.GetAttachmentsAsync(
                                    found.ProjectSnapshot.IssueContainer,
                                    found.ID);

                                foreach (var attachment in attachments.Where(
                                    x => project.Items.ContainsKey(x.Attributes.Urn)))
                                {
                                    found.Items.Add(
                                        attachment.ID,
                                        project.Items[attachment.Attributes.Urn]);
                                }

                                found.ProjectSnapshot.Issues.Add(found.Entity.ID, found);

                                break;
                            }
                        }
                    }

                    if (found != null)
                        result.Add(await converterToDto.Convert(found));
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
            => snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID)
               .Value;
    }
}
