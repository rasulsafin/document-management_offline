using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.General
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

        [JsonConstructor]
        public PagedList(IEnumerable<T> items, PagedData pageData)
        {
            PageData = pageData;
            Items = items;
        }

        [JsonProperty]
        public PagedData PageData { get; private set; }

        [JsonProperty]
        public IEnumerable<T> Items { get; private set; }
    }
}
