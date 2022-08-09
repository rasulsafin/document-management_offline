using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class IssueSnapshotUtilities
    {
        private readonly IIssuesService issuesService;

        public IssueSnapshotUtilities(
            IIssuesService issuesService)
        {
            this.issuesService = issuesService;
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
                result.Add(FillCommentAuthor(comment, project.HubSnapshot));

            return result;
        }

        public CommentSnapshot FillCommentAuthor(Comment comment, HubSnapshot hub)
        {
            hub.Users.TryGetValue(comment.Attributes.CreatedBy, out var author);
            return new CommentSnapshot(comment)
            {
                Author = author == null ? MrsConstants.DEFAULT_AUTHOR_NAME : author.Name,
            };
        }
    }
}
