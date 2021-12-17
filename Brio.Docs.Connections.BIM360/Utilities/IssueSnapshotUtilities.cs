using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;

namespace Brio.Docs.Connections.Bim360.Utilities
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

        public async Task<Dictionary<string, Attachment>> GetAttachments(
            IssueSnapshot issueSnapshot,
            ProjectSnapshot project)
            => await issuesService.GetAttachmentsAsync(project.IssueContainer, issueSnapshot.ID)
               .Where(x => x.Attributes.UrnType == UrnType.Oss || project.Items.ContainsKey(x.Attributes.Urn))
               .ToDictionaryAsync(attachment => attachment.ID);

        public async Task<List<CommentSnapshot>> GetComments(IssueSnapshot issueSnapshot,
           ProjectSnapshot project)
        {
            if (issueSnapshot.Entity.Attributes.CommentCount == 0)
                return new List<CommentSnapshot>();

            var result = new List<CommentSnapshot>();

            await foreach (var comment in issuesService.GetCommentsAsync(project.IssueContainer, issueSnapshot.ID))
                result.Add(await FillCommentAuthor(comment, project.HubSnapshot.Entity));

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
