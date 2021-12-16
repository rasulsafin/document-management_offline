using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class CommentsBimElementsConverter : IConverter<IEnumerable<Comment>, IEnumerable<BimElementExternalDto>>
    {
        private readonly MetaCommentHelper metaCommentHelper;

        public CommentsBimElementsConverter(MetaCommentHelper metaCommentHelper)
            => this.metaCommentHelper = metaCommentHelper;

        public Task<IEnumerable<BimElementExternalDto>> Convert(IEnumerable<Comment> comments)
            => metaCommentHelper.TryGet(
                comments,
                MrsConstants.BIM_ELEMENTS_META_COMMENT_TAG,
                out IEnumerable<BimElementExternalDto> result)
                ? Task.FromResult(result)
                : Task.FromResult<IEnumerable<BimElementExternalDto>>(Array.Empty<BimElementExternalDto>());
    }
}
