using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Forge.Models;
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
            var allIssues = await issuesService.GetIssuesAsync(
                containerID,
                parameters.Append(
                    new QueryParameter("fields[quality_issues]", "closed_at,comment_count,attachment_count")));
            var filters = new IQueryParameter[]
            {
                new Filter(Constants.FILTER_KEY_ISSUE_UPDATED_AFTER, updatedAfter.ToString("O")),
            };

            foreach (var issue in allIssues.Where(x => x.Attributes.ClosedAt < updatedAfter))
            {
                if (issue.Attributes.AttachmentCount != 0)
                {
                    var comments = await issuesService.GetCommentsAsync(containerID, issue.ID, filters);
                    if (comments.Any())
                        yield return issue.ID;
                }

                if (issue.Attributes.AttachmentCount != 0)
                {
                    var attachments = await issuesService.GetAttachmentsAsync(containerID, issue.ID, filters);
                    if (attachments.Any())
                        yield return issue.ID;
                }
            }
        }
    }
}
