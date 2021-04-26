using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Exceptions;

namespace MRS.DocumentManagement.Utility.Extensions
{
    internal static class FindExtensions
    {
        public static async Task<T> FindOrThrowAsync<T>(
            this DbSet<T> set,
            int id,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            var result = await set.FindAsync(new object[] { id }, cancellationToken);
            if (result == null)
                throw new NotFoundException<T>(id);

            return result;
        }

        public static async Task<T> FindOrThrowAsync<T>(
            this DbContext dbContext,
            int id,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            var result = await dbContext.FindAsync<T>(new object[] { id }, cancellationToken);
            if (result == null)
                throw new NotFoundException<T>(id);

            return result;
        }

        public static TValue FindOrThrow<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key)
        {
            if (dictionary.TryGetValue(key, out var result))
                return result;

            throw new NotFoundException<TValue>(nameof(key), key.ToString());
        }

        public static async Task<T> FindOrThrowAsync<T>(
            this IQueryable<T> set,
            string propertyName,
            object propertyValue,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var predicate = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    Expression.Property(parameterExpression, propertyName),
                    Expression.Constant(propertyValue)),
                parameterExpression);
            var result = await set.FirstOrDefaultAsync(predicate, cancellationToken);
            if (result == null)
                throw new NotFoundException<T>(propertyName, propertyValue.ToString());

            return result;
        }

        public static async Task<T> FindWithIgnoreCaseOrThrowAsync<T>(
            this IQueryable<T> set,
            string propertyName,
            string propertyValue,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var method = typeof(string).GetMethod(nameof(string.ToLower), Array.Empty<Type>());

            var predicate = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    Expression.Call(Expression.Property(parameterExpression, propertyName), method!),
                    Expression.Call(Expression.Constant(propertyValue), method)),
                parameterExpression);
            var result = await set.FirstOrDefaultAsync(predicate, cancellationToken);
            if (result == null)
                throw new NotFoundException<T>(propertyName, propertyValue);

            return result;
        }
    }
}
