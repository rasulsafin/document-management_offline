using System.Linq;

namespace MRS.DocumentManagement.Utility.Pagination
{
    internal static class PagedListExtensions
    {
        internal static PagedList<T> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
