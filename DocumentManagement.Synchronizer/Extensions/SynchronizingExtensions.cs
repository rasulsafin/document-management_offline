using System;
using System.Reflection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Synchronizer.Models;

namespace MRS.DocumentManagement.Synchronizer.Extensions
{
    internal static class SynchronizingExtensions
    {
        public static SynchronizingAction DetermineAction<T>(this SynchronizingTuple<T> tuple)
                where T : ISynchronizable<T>
        {
            return tuple.Synchronized == null && tuple.Local == null     ? SynchronizingAction.AddToLocal
                    : tuple.Synchronized == null && tuple.Remote == null ? SynchronizingAction.AddToRemote
                    : tuple.Local == null && tuple.Remote == null        ? SynchronizingAction.RemoveFromLocal
                    : tuple.Local == null && tuple.HasExternalID         ? SynchronizingAction.RemoveFromRemote
                    : tuple.Remote == null && tuple.HasExternalID        ? SynchronizingAction.RemoveFromLocal
                                                                           : SynchronizingAction.Merge;
        }

        public static void Merge<T>(this SynchronizingTuple<T> tuple)
                where T : class, ISynchronizable<T>, new()
        {
            var properties = typeof(T).GetProperties();

            tuple.Local ??= new T();
            tuple.Remote ??= new T();
            tuple.Synchronized ??= new T { IsSynchronized = true };
            tuple.LinkEntities();

            void UpdateValue(PropertyInfo property, object value)
            {
                property.SetValue(tuple.Local, value);
                property.SetValue(tuple.Remote, value);
                property.SetValue(tuple.Synchronized, value);
            }

            foreach (var property in properties)
            {
                if (typeof(int).Namespace != property.PropertyType.Namespace
                    || property.Name.Contains("id", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var synchronizedValue = property.GetValue(tuple.Synchronized);
                var localValue = property.GetValue(tuple.Local);
                var remoteValue = property.GetValue(tuple.Remote);

                var value = (Equals(localValue, remoteValue) || Equals(synchronizedValue, remoteValue)) ? localValue
                        : Equals(synchronizedValue, localValue)                                         ? remoteValue
                        : tuple.Local.UpdatedAt > tuple.Remote.UpdatedAt                                ? localValue
                            : remoteValue;

                UpdateValue(property, value);
            }
        }

        private static void LinkEntities<T>(this SynchronizingTuple<T> tuple)
                where T : class, ISynchronizable<T>, new()
        {
            tuple.Local.ExternalID = tuple.Synchronized.ExternalID = tuple.ExternalID;
            tuple.Local.SynchronizationMateID = tuple.Synchronized.ID;
            tuple.Synchronized.SynchronizationMateID = tuple.Local.ID;
        }
    }
}
