using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Properties;
using Brio.Docs.Connections.Bim360.Synchronization.Models;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class LinkedInfoCommentsConverter
        : IConverter<CommentCreatingData<LinkedInfo>, IEnumerable<Comment>>
    {
        private readonly MetaCommentHelper metaCommentHelper;

        public LinkedInfoCommentsConverter(MetaCommentHelper metaCommentHelper)
            => this.metaCommentHelper = metaCommentHelper;

        public Task<IEnumerable<Comment>> Convert(CommentCreatingData<LinkedInfo> commentCreatingData)
        {
            var info = commentCreatingData.IsPreviousDataEmpty
                ? Comments.OriginalModelAddedInfo
                : Comments.OriginalModelChangedInfo;

            var result = metaCommentHelper.CreateComments(
                commentCreatingData.Data,
                MrsConstants.LINKED_INFO_META_COMMENT_TAG,
                info);
            return Task.FromResult(result);
        }
    }
}
