using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Version = Brio.Docs.Connections.Bim360.Forge.Models.DataManagement.Version;

namespace Brio.Docs.Connections.Bim360.Synchronizers
{
    internal class Bim360ObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly SnapshotGetter snapshot;
        private readonly SnapshotUpdater snapshotUpdater;
        private readonly IssuesService issuesService;
        private readonly IAccessController accessController;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly SnapshotFiller filler;
        private readonly IssueSnapshotUtilities snapshotUtilities;
        private readonly IConverter<ObjectiveExternalDto, Issue> converterToIssue;
        private readonly IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto;

        public Bim360ObjectivesSynchronizer(
            SnapshotGetter snapshot,
            SnapshotUpdater snapshotUpdater,
            IssuesService issuesService,
            IAccessController accessController,
            ItemsSyncHelper itemsSyncHelper,
            SnapshotFiller filler,
            IssueSnapshotUtilities snapshotUtilities,
            IConverter<ObjectiveExternalDto, Issue> converterToIssue,
            IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto)
        {
            this.snapshot = snapshot;
            this.snapshotUpdater = snapshotUpdater;
            this.itemsSyncHelper = itemsSyncHelper;
            this.issuesService = issuesService;
            this.converterToIssue = converterToIssue;
            this.converterToDto = converterToDto;
            this.filler = filler;
            this.accessController = accessController;
            this.snapshotUtilities = snapshotUtilities;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);

            var issue = await converterToIssue.Convert(obj);
            var project = snapshot.GetProject(obj.ProjectExternalID);
            var created = await issuesService.PostIssueAsync(project.IssueContainer, issue);
            var issueSnapshot = snapshotUpdater.CreateIssue(project, created);
            var parsedToDto = await converterToDto.Convert(issueSnapshot);
            await AddItems(obj, project, issueSnapshot, parsedToDto);

            return parsedToDto;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);

            var issue = await converterToIssue.Convert(obj);
            var project = snapshot.GetProject(obj.ProjectExternalID);
            var issueSnapshot = snapshot.GetIssue(project, obj.ExternalID);

            if (!issue.Attributes.PermittedStatuses.Contains(Status.Void))
            {
                issue.Attributes.Status = Status.Open;
                issue = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            }

            issue.Attributes.Status = Status.Void;
            issue = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            snapshotUpdater.UpdateIssue(issueSnapshot, issue);

            return await converterToDto.Convert(issueSnapshot);
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);

            var project = snapshot.GetProject(obj.ProjectExternalID);
            var issueSnapshot = snapshot.GetIssue(project, obj.ExternalID);
            var issue = await converterToIssue.Convert(obj);
            issue = await issuesService.PatchIssueAsync(project.IssueContainer, issue);
            snapshotUpdater.UpdateIssue(issueSnapshot, issue);

            var newComment = obj.DynamicFields.FirstOrDefault(x => x.ExternalID == MrsConstants.NEW_COMMENT_ID && x.Value != string.Empty);
            if (newComment != null)
            {
                var comment = await PostComment(newComment, issueSnapshot.ID, project.IssueContainer);
                if (comment != null)
                    issueSnapshot.Comments.Add(await snapshotUtilities.FillCommentAuthor(comment, project.HubSnapshot.Entity));
            }

            var parsedToDto = await converterToDto.Convert(issueSnapshot);
            await AddItems(obj, project, issueSnapshot, parsedToDto);

            return parsedToDto;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);
            await UpdateSnapshot(date);
            return snapshot.GetIssues().Where(x => x.Entity.Attributes.UpdatedAt > date)
               .Select(x => x.ID)
               .ToList();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await accessController.CheckAccessAsync(CancellationToken.None);

            var result = new List<ObjectiveExternalDto>();

            foreach (var id in ids)
            {
                try
                {
                    var found = snapshot.GetIssue(id);

                    if (found == null)
                    {
                        foreach (var project in snapshot.GetProjects())
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

                                found = snapshotUpdater.CreateIssue(project, received);
                                found.Attachments = await snapshotUtilities.GetAttachments(found, project);
                                found.Comments = await snapshotUtilities.GetComments(found, project);
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

        private async Task AddItems(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            IssueSnapshot issueSnapshot,
            ObjectiveExternalDto parsedToDto)
        {
            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, project, issueSnapshot.Entity);
                issueSnapshot.Attachments = added.ToDictionary(x => x.ID);
                parsedToDto.Items = issueSnapshot.Attachments.Values.Select(i => i.ToDto()).ToList();
            }
        }

        private async Task<IEnumerable<Attachment>> AddItems(
            ICollection<ItemExternalDto> items,
            ProjectSnapshot project,
            Issue issue)
        {
            var resultItems = new List<Attachment>();
            var attachments = await issuesService.GetAttachmentsAsync(project.IssueContainer, issue.ID);

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

        private async Task UpdateSnapshot(DateTime date)
        {
            await filler.UpdateStatusesConfigIfNull();
            await filler.UpdateIssuesIfNull(date);
            await filler.UpdateIssueTypes();
            await filler.UpdateRootCauses();
            await filler.UpdateLocations();
            await filler.UpdateAssignTo();
            await filler.UpdateStatuses();
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
