﻿using System.Collections.Generic;

namespace MRS.DocumentManagement.Synchronizer.Extensions
{
    internal static class CollectionExtentions
    {
        public static void AddIsNotNull<T>(this ICollection<T> collection, T item)
        {
            if (item == null)
                return;

            collection.Add(item);
        }
    }
}
