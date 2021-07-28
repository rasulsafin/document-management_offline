using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Utility.Pagination
{
    public class PagedList<T>
    {
        public PagedList(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
        {
            PageData = new PagedData(
                currentPage,
                pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize),
                totalCount);

            Items = items;
        }

        public PagedData PageData { get; private set; }

        public IEnumerable<T> Items { get; private set; }
    }
}
