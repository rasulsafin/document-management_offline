using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class PagedListDto<T>
    {
        public PagedDataDto PageData { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}
