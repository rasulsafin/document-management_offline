using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Exceptions;

namespace MRS.DocumentManagement.Utility.Extensions
{
    internal static class DBExtensions
    {
        public static async Task<T> FindOrThrowAsync<T>(this DbSet<T> set, int id, CancellationToken cancellationToken = CancellationToken.)
            where T : class
        {
            var result = await set.FindAsync(id);
            if (result == null)
                throw new NotFoundException<T>(id);

            return result;
        }

        public static async Task<T> FindOrThrowAsync<T>(this IQueryable<T> set, Expression<Func<T, bool>> predicate, int id)
            where T : class
        {
            var result = await set.FirstOrDefaultAsync(predicate);
            if (result == null)
                throw new NotFoundException<T>(id);

            return result;
        }
    }
}
