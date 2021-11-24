using System.Collections.Generic;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    public struct CommentCreatingData
    {
        public IEnumerable<BimElementExternalDto> Data { get; set; }

        public bool IsPreviousDataEmpty { get; set; }
    }
}
