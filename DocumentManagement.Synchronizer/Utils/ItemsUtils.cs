using System;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Utils
{
    internal class ItemsUtils
    {
        public static T GetLinked<T>(Item item, object parent, EntityType entityType)
            where T : ISynchronizable<T>
        {
            var tuple = (SynchronizingTuple<T>)parent;
            T linked;

            switch (entityType)
            {
                case EntityType.Local:
                    linked = tuple.Local;
                    tuple.LocalChanged = true;
                    break;
                case EntityType.Synchronized:
                    linked = tuple.Synchronized;
                    tuple.SynchronizedChanged = true;
                    break;
                case EntityType.Remote:
                    linked = tuple.Remote;
                    tuple.RemoteChanged = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            if (linked == null)
            {
                throw new ArgumentException(
                    $"Parent doesn't contain {entityType} project");
            }

            return linked;
        }
    }
}
