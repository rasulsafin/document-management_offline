using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
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

        public async Task<Dictionary<string, Attachment>> GetAttachments(IssueSnapshot issueSnapshot,
            ProjectSnapshot project)
        {
            var result = new Dictionary<string, Attachment>();
            var attachments = await issuesService.GetAttachmentsAsync(
                project.IssueContainer,
                issueSnapshot.ID);

            foreach (var attachment in attachments.Where(
                                    x => x.Attributes.UrnType == UrnType.Oss ||
                                        project.Items.ContainsKey(x.Attributes.Urn)))
               result.Add(attachment.ID, attachment);

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
                    var commentSnapshot = await FillCommentAuthor(comment, project.HubSnapshot.Entity);
                    result.Add(commentSnapshot);
                }
            }

            return result;
        }

        public async Task<CommentSnapshot> FillCommentAuthor(Comment comment, Hub hub)
        {
            var author = (await accountAdminService.GetAccountUsersAsync(hub)).FirstOrDefault(u => u.Uid == comment.Attributes.CreatedBy);
            return new CommentSnapshot(comment)
            {
                Author = author == null ? MrsConstants.DEFAULT_AUTHOR_NAME : author.Name,
            };
        }
    }
}
