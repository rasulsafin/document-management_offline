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
        public async IAsyncEnumerable<string> GetUpdatedIssueIds(string containerID, DateTime updatedAfter, IEnumerable<IQueryParameter> parameters)
        {
            var closedAtField = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.ClosedAt);
            var commentCountField = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.CommentCount);
            var attachmentCountField = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.AttachmentCount);
            var fieldsParameter = new QueryParameter(
                "fields[quality_issues]",
                $"{closedAtField},{commentCountField},{attachmentCountField}");

            var allIssues = issuesService.GetIssuesAsync(
                containerID,
                parameters.Append(fieldsParameter));
            var filters = new IQueryParameter[]
            {
                new Filter(Constants.FILTER_KEY_ISSUE_UPDATED_AFTER, updatedAfter.ToString("O")),
            };

            await foreach (var issue in allIssues)
            {
                if (issue.Attributes.ClosedAt < updatedAfter)
                    continue;

                if (issue.Attributes.AttachmentCount != 0)
                {
                    var comments = issuesService.GetCommentsAsync(containerID, issue.ID, filters);
                    if (await comments.AnyAsync())
                        yield return issue.ID;
                }

                if (issue.Attributes.AttachmentCount != 0)
                {
                    var attachments = issuesService.GetAttachmentsAsync(containerID, issue.ID, filters);
                    if (await attachments.AnyAsync())
                        yield return issue.ID;
                }
            }
        }
    }
}
