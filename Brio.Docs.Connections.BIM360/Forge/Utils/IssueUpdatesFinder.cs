using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Services;

namespace Brio.Docs.Connections.Bim360.Forge.Utils
{
    internal class IssueUpdatesFinder
    {
        private readonly IssuesService issuesService;

        public IssueUpdatesFinder(IssuesService issuesService)
            => this.issuesService = issuesService;

        // Deleting an attachment is not defined as a change.
        public async IAsyncEnumerable<string> GetUpdatedIssueIds(
            string containerID,
            DateTime updatedAfter,
            IEnumerable<IQueryParameter> parameters)
        {
            var fieldsParameter = new FieldsFilter<Issue, Issue.IssueAttributes, Issue.IssueRelationships>(
                x => x.ClosedAt,
                x => x.CommentCount,
                x => x.AttachmentCount,
                x => x.UpdatedAt);

            var allIssues = issuesService.GetIssuesAsync(
                containerID,
                parameters.Append(fieldsParameter));

            var filter = new Filter(Constants.FILTER_KEY_ISSUE_UPDATED_AFTER, updatedAfter.ToString("O"));

            var commentParameters = new IQueryParameter[]
            {
                filter, new FieldsFilter<Comment, Comment.CommentAttributes, object>(x => x.CreatedAt),
            };

            var attachmentParameters = new IQueryParameter[]
            {
                filter,
                new FieldsFilter<Attachment, Attachment.AttachmentAttributes, Attachment.AttachmentRelationships>(
                    x => x.CreatedAt),
            };

            await foreach (var issue in allIssues)
            {
                if (issue.Attributes.UpdatedAt > updatedAfter)
                    yield return issue.ID;

                if (issue.Attributes.ClosedAt < updatedAfter)
                    continue;

                if (issue.Attributes.AttachmentCount != 0)
                {
                    var comments = issuesService.GetCommentsAsync(containerID, issue.ID, commentParameters);
                    if (await comments.AnyAsync())
                        yield return issue.ID;
                }

                if (issue.Attributes.AttachmentCount != 0)
                {
                    var attachments = issuesService.GetAttachmentsAsync(containerID, issue.ID, attachmentParameters);
                    if (await attachments.AnyAsync())
                        yield return issue.ID;
                }
            }
        }
    }
}
