using System;
using System.Collections.Generic;
using System.Linq;

namespace MRS.DocumentManagement.Connection.Tdms
{
    internal static class CollectionExtensions
    {
        public static void AddIsNotNull<T>(this ICollection<T> collection, T item)
        {
            if (item == null)
                return;

            collection.Add(item);
        }
    }
}
