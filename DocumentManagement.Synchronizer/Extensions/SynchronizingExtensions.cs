using System;
using System.Reflection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Extensions
{
    internal static class SynchronizingExtensions
    {
        public static SynchronizingAction DetermineAction<T>(this SynchronizingTuple<T> tuple)
                where T : ISynchronizable<T>
            => DetermineAction((tuple.Local, tuple.Synchronized, tuple.Remote));

        public static SynchronizingAction DetermineAction<T>(this (T local, T synchronized, T remote) tuple)
            => tuple.synchronized == null && tuple.local == null     ? SynchronizingAction.AddToLocal
                : tuple.synchronized == null && tuple.remote == null ? SynchronizingAction.AddToRemote
                : tuple.local == null && tuple.remote == null        ? SynchronizingAction.RemoveFromLocal
                : tuple.local == null                                ? SynchronizingAction.RemoveFromRemote
                : tuple.remote == null                               ? SynchronizingAction.RemoveFromLocal
                                                                       : SynchronizingAction.Merge;

        public static void Merge<T>(this SynchronizingTuple<T> tuple)
                where T : class, ISynchronizable<T>, new()
        {
            var properties = typeof(T).GetProperties();

            tuple.Local ??= new T();
            tuple.Remote ??= new T();
            tuple.Synchronized ??= new T { IsSynchronized = true };
            tuple.LinkEntities();

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute(typeof(ForbidMergeAttribute)) != null)
                        continue;

                var synchronizedValue = property.GetValue(tuple.Synchronized);
                var localValue = property.GetValue(tuple.Local);
                var remoteValue = property.GetValue(tuple.Remote);

                var localSynchronizedAndNotChanged = Equals(localValue, remoteValue) || Equals(synchronizedValue, remoteValue);
                var localNotChanged = Equals(synchronizedValue, localValue);
                var localMoreRelevant = tuple.Local.UpdatedAt > tuple.Remote.UpdatedAt;

                var value = localSynchronizedAndNotChanged   ? localValue
                        : localNotChanged                   ? remoteValue
                        : localMoreRelevant                 ? localValue
                        : remoteValue;

                UpdateValue(tuple, property, (localValue, synchronizedValue, remoteValue), value);
            }
        }

        public static object GetPropertyValue<T>(this SynchronizingTuple<T> tuple, string propertyName)
            where T : class, ISynchronizable<T>, new()
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException(nameof(GetPropertyValue), nameof(propertyName));

            bool TryGetValue(T source, out object value)
            {
                value = null;
                if (source == null)
                    return false;

                value = propertyInfo.GetValue(source);
                return value != default;
            }

            return TryGetValue(tuple.Local, out var result1)     ? result1 :
                TryGetValue(tuple.Remote, out var result2)       ? result2 :
                TryGetValue(tuple.Synchronized, out var result3) ? result3 : null;
        }

        private static void LinkEntities<T>(this SynchronizingTuple<T> tuple)
                where T : class, ISynchronizable<T>, new()
        {
            tuple.Local.ExternalID = tuple.Synchronized.ExternalID = tuple.ExternalID;
            tuple.Local.SynchronizationMate = tuple.Synchronized;
        }

        private static void UpdateValue<T>(T obj, PropertyInfo property, object oldValue, object newValue, Action action)
        {
            if (!Equals(oldValue, newValue))
            {
                property.SetValue(obj, newValue);
                action();
            }
        }

        private static void UpdateValue<T>(
            SynchronizingTuple<T> tuple,
            PropertyInfo property,
            (object local, object synhronzied, object remote) oldValues,
            object value)
            where T : ISynchronizable<T>
        {
            UpdateValue(tuple.Local, property, oldValues.local, value, () => tuple.LocalChanged = true);
            UpdateValue(
                tuple.Synchronized,
                property,
                oldValues.synhronzied,
                value,
                () => tuple.SynchronizedChanged = true);
            UpdateValue(tuple.Remote, property, oldValues.remote, value, () => tuple.RemoteChanged = true);
        }
    }
}
