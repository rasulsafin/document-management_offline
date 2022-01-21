using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Synchronization.Models;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class CommentsLinkedInfoConverter
        : IConverter<IEnumerable<Comment>, LinkedInfo>
    {
        private readonly MetaCommentHelper metaCommentHelper;

        public CommentsLinkedInfoConverter(MetaCommentHelper metaCommentHelper)
            => this.metaCommentHelper = metaCommentHelper;

        public Task<LinkedInfo> Convert(IEnumerable<Comment> comments)
            => metaCommentHelper.TryGet(
                comments,
                MrsConstants.LINKED_INFO_META_COMMENT_TAG,
                out LinkedInfo result)
                ? Task.FromResult(result)
                : Task.FromResult<LinkedInfo>(null);
    }
}
