using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class IssueSnapshotUtilities
    {
        private readonly IIssuesService issuesService;
        private readonly AccountAdminService accountAdminService;

        public IssueSnapshotUtilities(IIssuesService issuesService,
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
