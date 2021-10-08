using System;
using System.Collections.Generic;

namespace Brio.Docs.Connections.Bim360.Synchronization.Extensions
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

            return -1;
        }
    }
}
