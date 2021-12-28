using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.Bim360.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Version = Brio.Docs.Connections.Bim360.Forge.Models.DataManagement.Version;

namespace Brio.Docs.Connections.Bim360.Synchronizers
{
    internal class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly Bim360Snapshot snapshot;
        private readonly IssuesService issuesService;
        private readonly Authenticator authenticator;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly SnapshotFiller filler;
        private readonly IssueSnapshotUtilities snapshotUtilities;
        private readonly IssueUpdatesFinder issueUpdatesFinder;
        private readonly IConverter<ObjectiveExternalDto, Issue> converterToIssue;
        private readonly IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto;

        public Bim360ObjectivesSynchronizer(
            Bim360Snapshot snapshot,
            IssuesService issuesService,
            Authenticator authenticator,
            ItemsSyncHelper itemsSyncHelper,
            SnapshotFiller filler,
            IssueSnapshotUtilities snapshotUtilities,
            IssueUpdatesFinder issueUpdatesFinder,
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
            this.snapshotUtilities = snapshotUtilities;
            this.issueUpdatesFinder = issueUpdatesFinder;
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
                issueSnapshot.Attachments = added.ToDictionary(x => x.ID);
                parsedToDto.Items = issueSnapshot.Attachments.Values.Select(i => i.ToDto()).ToList();
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

            var newComment = obj.DynamicFields.FirstOrDefault(x => x.ExternalID == MrsConstants.NEW_COMMENT_ID && x.Value != string.Empty);
            if (newComment != null)
            {
                var comment = await PostComment(newComment, issueSnapshot.ID, project.IssueContainer);
                if (comment != null)
                    issueSnapshot.Comments.Add(await snapshotUtilities.FillCommentAuthor(comment, project.HubSnapshot.Entity));
            }

            var parsedToDto = await converterToDto.Convert(issueSnapshot);
            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, project, issueSnapshot.Entity);
                issueSnapshot.Attachments = added.ToDictionary(x => x.ID);
                parsedToDto.Items = issueSnapshot.Attachments.Values.Select(i => i.ToDto()).ToList();
            }

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            await filler.UpdateProjectsIfNull();
            await filler.UpdateIssuesIfNull(date);

            var result = new List<string>();

            foreach (var projectSnapshot in snapshot.ProjectEnumerable)
            {
                await foreach (var id in issueUpdatesFinder.GetUpdatedIssueIds(
                                   projectSnapshot.IssueContainer,
                                   date,
                                   new[] { IssueUtilities.GetFilterForUnremoved() }))
                    result.Add(id);
            }

            return result;
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await authenticator.CheckAccessAsync(CancellationToken.None);

            await filler.UpdateStatusesConfigIfNull();
            await filler.UpdateIssueTypes();
            await filler.UpdateRootCauses();
            await filler.UpdateLocations();
            await filler.UpdateAssignTo();
            await filler.UpdateStatuses();

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

                                found = new IssueSnapshot(received, project);
                                found.Attachments = await snapshotUtilities.GetAttachments(found, project);
                                found.Comments = await snapshotUtilities.GetComments(found, project);
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

        private async Task<IEnumerable<Attachment>> AddItems(
            ICollection<ItemExternalDto> items,
            ProjectSnapshot project,
            Issue issue)
        {
            var resultItems = new List<Attachment>();
            var attachments = await issuesService.GetAttachmentsAsync(project.IssueContainer, issue.ID).ToListAsync();

            foreach (var item in items)
            {
                // If item with the same name already exists add existing item
                var itemWithSameNameExists = project.FindItemByName(item.FileName);

                if (itemWithSameNameExists != null)
                {
                    var attachment = attachments.FirstOrDefault(x => x.Attributes.Name == item.FileName);

                    if (attachment != null)
                    {
                        resultItems.Add(attachment);
                        continue;
                    }

                    var attached = item.ItemType == ItemType.Media
                        ? await AttachPhoto(itemWithSameNameExists.Version, issue.ID, project.IssueContainer)
                        : await AttachItem(itemWithSameNameExists.Entity, issue.ID, project.IssueContainer);
                    if (attached != null)
                        resultItems.Add(attached);
                }
                else
                {
                    var uploadedItem = await itemsSyncHelper.PostItem(project, item);

                    if (uploadedItem == default)
                        continue;

                    await Task.Delay(5000);
                    var attached = item.ItemType == ItemType.Media
                        ? await AttachPhoto(uploadedItem.Version, issue.ID, project.IssueContainer)
                        : await AttachItem(uploadedItem.Entity, issue.ID, project.IssueContainer);
                    if (attached != null)
                        resultItems.Add(attached);
                }
            }

            return resultItems;
        }

        private async Task<Attachment> AttachItem(Item posted, string issueId, string containerId)
        {
            var attachment = new Attachment
            {
                Attributes = new Attachment.AttachmentAttributes
                {
                    Name = posted.Attributes.DisplayName,
                    IssueId = issueId,
                    Urn = posted.ID,
                    UrnType = UrnType.DM,
                },
            };

            try
            {
                return await issuesService.PostIssuesAttachmentsAsync(containerId, attachment);
            }
            catch
            {
                return null;
            }
        }

        private async Task<Attachment> AttachPhoto(Version version, string issueId, string containerId)
        {
            var attachment = new Attachment
            {
                Attributes = new Attachment.AttachmentAttributes
                {
                    Name = version.Attributes.Name,
                    IssueId = issueId,
                    Urn = version.GetStorage().ID,
                    UrnType = UrnType.Oss,
                },
            };

            try
            {
                return await issuesService.PostIssuesAttachmentsAsync(containerId, attachment);
            }
            catch
            {
                return null;
            }
        }

        private ProjectSnapshot GetProjectSnapshot(ObjectiveExternalDto obj)
            => snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID)
               .Value;

        private async Task<Comment> PostComment(DynamicFieldExternalDto commentDto, string issueId, string containerId)
        {
            var comment = new Comment
            {
                Attributes = new Comment.CommentAttributes
                {
                    IssueId = issueId,
                    Body = commentDto.Value,
                },
            };

            try
            {
                return await issuesService.PostIssuesCommentsAsync(containerId, comment);
            }
            catch
            {
                return null;
            }
        }
    }
}
