using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions
{
    internal static class EnumerableExtensions
    {
        public static int IndexOfFirst<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
        {
            var i = 0;

            foreach (var item in enumerable)
            {
                if (predicate(item))
                    return i;
                i++;
            }

            throw new InvalidOperationException("Sequence contains no matching element");
        }
    }
}
