using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MRS.DocumentManagement.Utility.Pagination
{
    internal static class PagedListExtensions
    {
        internal static IQueryable<T> ByPages<T, TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> orderBy, int pageNumber, int pageSize, bool orderByDescending = false)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            source = orderByDescending ? source.OrderByDescending(orderBy) : source.OrderBy(orderBy);
            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }
}
