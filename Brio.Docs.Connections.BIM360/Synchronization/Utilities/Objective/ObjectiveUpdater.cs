using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Version = Brio.Docs.Connections.Bim360.Forge.Models.DataManagement.Version;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities.Objective
{
    internal class ObjectiveUpdater
    {
        private readonly SnapshotGetter snapshot;
        private readonly SnapshotUpdater snapshotUpdater;
        private readonly IssuesService issuesService;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly IssueSnapshotUtilities snapshotUtilities;
        private readonly MetaCommentHelper metaCommentHelper;
        private readonly IConverter<ObjectiveExternalDto, Issue> converterToIssue;
        private readonly IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto;

        public ObjectiveUpdater(
            SnapshotGetter snapshot,
            SnapshotUpdater snapshotUpdater,
            IssuesService issuesService,
            ItemsSyncHelper itemsSyncHelper,
            IssueSnapshotUtilities snapshotUtilities,
            MetaCommentHelper metaCommentHelper,
            IConverter<ObjectiveExternalDto, Issue> converterToIssue,
            IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto)
        {
            this.snapshot = snapshot;
            this.snapshotUpdater = snapshotUpdater;
            this.issuesService = issuesService;
            this.itemsSyncHelper = itemsSyncHelper;
            this.snapshotUtilities = snapshotUtilities;
            this.metaCommentHelper = metaCommentHelper;
            this.converterToIssue = converterToIssue;
            this.converterToDto = converterToDto;
        }

        public async Task<ObjectiveExternalDto> Put(ObjectiveExternalDto obj)
        {
            var project = snapshot.GetProject(obj.ProjectExternalID);
            var issue = await converterToIssue.Convert(obj);
            var isNew = IsNew(issue);

            IssueSnapshot issueSnapshot;

            if (isNew || !IssueUtilities.ReadOnlyStatuses.Contains(issue.Attributes.Status))
            {
                issueSnapshot = await Put(issue, project, isNew);

                await AddComment(obj, issueSnapshot, project);
                await AddItems(obj, project, issueSnapshot);
                await AddBimElements(obj, project, issueSnapshot);
            }
            else
            {
                issueSnapshot = snapshot.GetIssue(project, obj.ExternalID);

                await AddComment(obj, issueSnapshot, project);
                await AddItems(obj, project, issueSnapshot);
                await AddBimElements(obj, project, issueSnapshot);

                issueSnapshot = await Put(issue, project, false);
            }

            var parsedToDto = await converterToDto.Convert(issueSnapshot);

            return parsedToDto;
        }

        private async Task AddComment(ObjectiveExternalDto obj, IssueSnapshot issueSnapshot, ProjectSnapshot project)
        {
            var newComment = obj.DynamicFields.FirstOrDefault(
                x
                    => x.ExternalID == MrsConstants.NEW_COMMENT_ID && !string.IsNullOrWhiteSpace(x.Value));

            if (newComment != null)
            {
                var comment = await PostComment(newComment, issueSnapshot.ID, project.IssueContainer);

                if (comment != null)
                {
                    issueSnapshot.Comments.Add(
                        await snapshotUtilities.FillCommentAuthor(comment, project.HubSnapshot.Entity));
                }
            }
        }

        private async Task<IssueSnapshot> Put(Issue issue, ProjectSnapshot project, bool isNew)
        {
            issue = await PutIssueAsync(project, issue, isNew);
            var issueSnapshot = UpdateSnapshot(project, issue, isNew);
            return issueSnapshot;
        }

        private IssueSnapshot UpdateSnapshot(ProjectSnapshot project, Issue issue, bool isNew)
            => isNew
                ? snapshotUpdater.CreateIssue(project, issue)
                : snapshotUpdater.UpdateIssue(project, issue);

        private async Task<Issue> PutIssueAsync(ProjectSnapshot project, Issue issue, bool isNew)
            => isNew
                ? await issuesService.PostIssueAsync(project.IssueContainer, issue)
                : await issuesService.PatchIssueAsync(project.IssueContainer, issue);

        private async Task AddItems(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            IssueSnapshot issueSnapshot)
        {
            if (obj.Items?.Any() ?? false)
            {
                var added = await AddItems(obj.Items, project, issueSnapshot.Entity);
                issueSnapshot.Attachments = added.ToDictionary(x => x.ID);
            }
        }

        private async Task AddBimElements(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            IssueSnapshot issueSnapshot)
        {
            var addedBimElements = await AddBimElements(obj.BimElements, project, issueSnapshot.Entity);
            addedBimElements ??= Enumerable.Empty<BimElementExternalDto>();
            issueSnapshot.BimElements = addedBimElements;
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

        private async Task<IEnumerable<BimElementExternalDto>> AddBimElements(
            ICollection<BimElementExternalDto> bimElements,
            ProjectSnapshot project,
            Issue issue)
        {
            var comments = await issuesService.GetCommentsAsync(project.IssueContainer, issue.ID);
            var currentElements = metaCommentHelper.GetBimElements(comments);
            var isCurrentEmpty = currentElements == null || !currentElements.Any();
            var isNewEmpty = bimElements == null || !bimElements.Any();

            if (!(isCurrentEmpty && isNewEmpty) && IsCollectionChanged())
            {
                try
                {
                    var newComments = metaCommentHelper.CreateComments(bimElements, isCurrentEmpty);

                    foreach (var comment in newComments)
                    {
                        comment.Attributes.IssueId = issue.ID;
                        await issuesService.PostIssuesCommentsAsync(project.IssueContainer, comment);
                    }
                }
                catch
                {
                    throw;
                }
            }

            return bimElements;

            bool IsCollectionChanged()
            {
                var currentOrdered = currentElements?.OrderBy(x => x.GlobalID) ??
                    Enumerable.Empty<BimElementExternalDto>();
                var newOrdered = bimElements?.OrderBy(x => x.GlobalID) ??
                    Enumerable.Empty<BimElementExternalDto>();
                return !currentOrdered.SequenceEqual(newOrdered, new BimElementComparer());
            }
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

        private bool IsNew(Issue issue)
            => issue.ID == null;

        private class BimElementComparer : IEqualityComparer<BimElementExternalDto>
        {
            public bool Equals(BimElementExternalDto x, BimElementExternalDto y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (ReferenceEquals(x, null))
                    return false;

                if (ReferenceEquals(y, null))
                    return false;

                return string.Equals(x.GlobalID, y.GlobalID, StringComparison.InvariantCulture) &&
                    string.Equals(x.ParentName, y.ParentName, StringComparison.InvariantCulture);
            }

            public int GetHashCode(BimElementExternalDto obj)
            {
                HashCode hashCode = new ();
                hashCode.Add(obj.GlobalID, StringComparer.InvariantCulture);
                hashCode.Add(obj.ParentName, StringComparer.InvariantCulture);
                return hashCode.ToHashCode();
            }
        }
    }
}
