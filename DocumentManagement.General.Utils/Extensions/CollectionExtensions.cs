using System.Collections.Generic;

namespace MRS.DocumentManagement.General.Utils.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddIsNotNull<T>(this ICollection<T> collection, T item)
        {
            if (item == null)
                return;

            collection.Add(item);
        }
    }
}
