using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Properties;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class BimElementsCommentsConverter
        : IConverter<CommentCreatingData<IEnumerable<BimElementExternalDto>>, IEnumerable<Comment>>
    {
        private readonly MetaCommentHelper metaCommentHelper;

        public BimElementsCommentsConverter(MetaCommentHelper metaCommentHelper)
            => this.metaCommentHelper = metaCommentHelper;

        public Task<IEnumerable<Comment>> Convert(
            CommentCreatingData<IEnumerable<BimElementExternalDto>> commentCreatingData)
        {
            var info = commentCreatingData.IsPreviousDataEmpty
                ? Comments.BimElementsAddedInfo
                : Comments.BimElementsChangedInfo;

            var result = metaCommentHelper.CreateComments(
                commentCreatingData.Data,
                MrsConstants.BIM_ELEMENTS_META_COMMENT_TAG,
                info);
            return Task.FromResult(result);
        }
    }
}
