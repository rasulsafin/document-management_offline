using System.Collections.Generic;

namespace Brio.Docs.Interface.Dtos
{
    public class PagedListDto<T>
    {
        public PagedDataDto PageData { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}
