using Brio.Docs.Database;
using Brio.Docs.Synchronization.Models;
using System;

namespace Brio.Docs.Synchronization.Utils
{
    internal class LinkingUtils
    {
        public static T CheckAndUpdateLinking<T>(object parent, EntityType entityType)
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
                throw new ArgumentException($"Parent doesn't contain {entityType} {typeof(T).Name}");

            return linked;
        }
    }
}
