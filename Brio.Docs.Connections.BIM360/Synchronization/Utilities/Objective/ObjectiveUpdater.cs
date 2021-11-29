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
using Brio.Docs.Connections.Bim360.Synchronization.Models;
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
        private readonly PushpinHelper pushpinHelper;
        private readonly IConverter<ObjectiveExternalDto, Issue> converterToIssue;
        private readonly IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto;

        private readonly LinkedInfoComparer linkedInfoComparer = new ();
        private readonly BimElementComparer bimElementComparer = new ();

        public ObjectiveUpdater(
            SnapshotGetter snapshot,
            SnapshotUpdater snapshotUpdater,
            IssuesService issuesService,
            ItemsSyncHelper itemsSyncHelper,
            IssueSnapshotUtilities snapshotUtilities,
            MetaCommentHelper metaCommentHelper,
            PushpinHelper pushpinHelper,
            IConverter<ObjectiveExternalDto, Issue> converterToIssue,
            IConverter<IssueSnapshot, ObjectiveExternalDto> converterToDto)
        {
            this.snapshot = snapshot;
            this.snapshotUpdater = snapshotUpdater;
            this.issuesService = issuesService;
            this.itemsSyncHelper = itemsSyncHelper;
            this.snapshotUtilities = snapshotUtilities;
            this.metaCommentHelper = metaCommentHelper;
            this.pushpinHelper = pushpinHelper;
            this.converterToIssue = converterToIssue;
            this.converterToDto = converterToDto;
        }

        public async Task<ObjectiveExternalDto> Put(ObjectiveExternalDto obj)
        {
            var project = snapshot.GetProject(obj.ProjectExternalID);
            var issue = await converterToIssue.Convert(obj);
            var pushpin = await Pushpin(project, obj, issue);
            issue = pushpin.issue;
            var isNew = IsNew(issue);
            issue = await PutIssueAsync(project, issue, isNew);
            var issueSnapshot = UpdateSnapshot(project, issue, isNew);

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

            var parsedToDto = await converterToDto.Convert(issueSnapshot);
            await AddItems(obj, project, issueSnapshot, parsedToDto);
            await AddBimElements(obj, project, issueSnapshot, parsedToDto);

            if (pushpin.linkedInfo != null)
                await AddLinkedInfo(pushpin.linkedInfo, project, issueSnapshot);

            return parsedToDto;
        }

        private async Task<(Issue issue, LinkedInfo linkedInfo)> Pushpin(ProjectSnapshot project, ObjectiveExternalDto obj, Issue issue)
        {
            var target = await GetTargetSnapshot(obj, project);
            var pushpin = await pushpinHelper.Pushpin(issue, obj, target);
            return pushpin;
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

        private async Task AddBimElements(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            IssueSnapshot issueSnapshot,
            ObjectiveExternalDto parsedToDto)
        {
            var addedBimElements = await AddBimElements(obj.BimElements, project, issueSnapshot);
            parsedToDto.BimElements = addedBimElements?.ToList();
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
            IssueSnapshot issue)
        {
            var comments = issue.Comments.Select(x => x.Entity);
            var currentElements = metaCommentHelper.GetBimElements(comments);
            var isCurrentEmpty = currentElements == null || !currentElements.Any();

            if (IsCollectionChanged(
                currentElements,
                bimElements,
                dtos => dtos.OrderBy(x => x.GlobalID),
                bimElementComparer))
            {
                try
                {
                    var newComments = metaCommentHelper.CreateComments(bimElements, isCurrentEmpty);
                    newComments = AddIssueId(newComments, issue.ID);

                    foreach (var comment in newComments)
                        await issuesService.PostIssuesCommentsAsync(project.IssueContainer, comment);
                }
                catch
                {
                    throw;
                }
            }

            return bimElements;
        }

        private async Task AddLinkedInfo(
            LinkedInfo linkedInfo,
            ProjectSnapshot project,
            IssueSnapshot issue)
        {
            var comments = issue.Comments.Select(x => x.Entity);
            var currentData = metaCommentHelper.GetLinkedInfo(comments);

            if (!linkedInfoComparer.Equals(currentData, linkedInfo))
            {
                try
                {
                    var newComments = metaCommentHelper.CreateComments(linkedInfo, currentData == null);
                    newComments = AddIssueId(newComments, issue.ID);

                    foreach (var comment in newComments)
                        await issuesService.PostIssuesCommentsAsync(project.IssueContainer, comment);
                }
                catch
                {
                    throw;
                }
            }
        }

        private IEnumerable<Comment> AddIssueId(IEnumerable<Comment> comments, string issueId)
        {
            foreach (var comment in comments)
            {
                comment.Attributes.IssueId = issueId;
                yield return comment;
            }
        }

        private async Task<ItemSnapshot> GetTargetSnapshot(ObjectiveExternalDto obj, ProjectSnapshot project)
        {
            if (obj.Location != null)
            {
                if (!project.Items.TryGetValue(obj.Location.Item.ExternalID, out var itemSnapshot))
                    itemSnapshot = project.FindItemByName(obj.Location.Item.FileName);

                if (itemSnapshot == null)
                {
                    var posted = await itemsSyncHelper.PostItem(project, obj.Location.Item);
                    itemSnapshot = project.Items[posted.ID];
                }

                return itemSnapshot;
            }

            if (obj.BimElements is { Count: > 0 })
            {
                return obj.BimElements.GroupBy(x => x.ParentName, (name, elements) => (name, count: elements.Count()))
                   .OrderByDescending(x => x.count)
                   .Select(x => x.name)
                   .Select(file => project.FindItemByName(file, true))
                   .FirstOrDefault(itemSnapshot => itemSnapshot != null);
            }

            return default;
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

        private bool IsCollectionChanged<T>(
            ICollection<T> oldCollection,
            ICollection<T> newCollection,
            Func<IEnumerable<T>, IOrderedEnumerable<T>> orderingFunc,
            IEqualityComparer<T> comparer)
        {
            var oldOrdered = oldCollection != null
                ? orderingFunc(oldCollection)
                : Enumerable.Empty<T>();
            var newOrdered = newCollection != null
                ? orderingFunc(newCollection)
                : Enumerable.Empty<T>();
            return !oldOrdered.SequenceEqual(newOrdered, comparer);
        }
    }
}
