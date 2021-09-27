using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class IssueSnapshotUtilities
    {
        private readonly IssuesService issuesService;
        private readonly AccountAdminService accountAdminService;

        public IssueSnapshotUtilities(IssuesService issuesService,
            AccountAdminService accountAdminService)
        {
            this.issuesService = issuesService;
            this.accountAdminService = accountAdminService;
        }

        public async Task<Dictionary<string, ItemSnapshot>> GetAttachments(IssueSnapshot issueSnapshot,
            ProjectSnapshot project)
        {
            var result = new Dictionary<string, ItemSnapshot>();
            var attachments = await issuesService.GetAttachmentsAsync(
                project.IssueContainer,
                issueSnapshot.ID);

            foreach (var attachment in attachments.Where(
                x => project.Items.ContainsKey(x.Attributes.Urn)))
            {
                result.Add(
                    attachment.ID,
                    project.Items[attachment.Attributes.Urn]);
            }

            return result;
        }

        public async Task<List<CommentSnapshot>> GetComments(IssueSnapshot issueSnapshot,
           ProjectSnapshot project)
        {
            var result = new List<CommentSnapshot>();
            if (issueSnapshot.Entity.Attributes.CommentCount > 0)
            {
                var comments = await issuesService.GetCommentsAsync(project.IssueContainer, issueSnapshot.ID);
                foreach (var comment in comments)
                {
                    var author = (await accountAdminService.GetAccountUsersAsync(project.HubSnapshot.Entity)).FirstOrDefault(u => u.Uid == comment.Attributes.CreatedBy);
                    result.Add(
                        new CommentSnapshot(comment)
                        {
                            Author = author == null ? MrsConstants.DEFAULT_AUTHOR_NAME : author.Name,
                        });
                }
            }

            return result;
        }
    }
}
