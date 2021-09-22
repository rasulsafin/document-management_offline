using Brio.Docs.Database;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using System;
using System.Reflection;

namespace Brio.Docs.Synchronization.Extensions
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
            MergePrivate(tuple, tuple.Local?.UpdatedAt ?? default, tuple.Remote?.UpdatedAt ?? default);
            tuple.LinkEntities();
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

        public static void SynchronizeChanges(this ISynchronizationChanges parentTuple, ISynchronizationChanges childTuple)
        {
            parentTuple.LocalChanged |= childTuple.LocalChanged;
            parentTuple.SynchronizedChanged |= childTuple.SynchronizedChanged;
            parentTuple.RemoteChanged |= childTuple.RemoteChanged;
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

        private static void MergePrivate<T>(SynchronizingTuple<T> tuple, DateTime localUpdatedAt, DateTime remoteUpdatedAt)
        {
            var properties = typeof(T).GetProperties();

            tuple.Local ??= (T)Activator.CreateInstance(typeof(T));
            tuple.Remote ??= (T)Activator.CreateInstance(typeof(T));
            tuple.Synchronized ??= (T)Activator.CreateInstance(typeof(T));

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute(typeof(ForbidMergeAttribute)) != null)
                    continue;

                if (NeedMergeSubtype(tuple, property))
                {
                    var type = typeof(SynchronizingTuple<>).MakeGenericType(property.PropertyType);
                    dynamic subtuple = Activator.CreateInstance(
                        type,
                        default(string),
                        property.GetValue(tuple.Synchronized),
                        property.GetValue(tuple.Local),
                        property.GetValue(tuple.Remote));

                    var merge = typeof(SynchronizingExtensions).GetMethod(nameof(MergePrivate), BindingFlags.Static | BindingFlags.NonPublic) !
                       .MakeGenericMethod(property.PropertyType);

                    var parameters = new[] { subtuple, localUpdatedAt, remoteUpdatedAt };
                    merge!.Invoke(null, parameters);
                    tuple.SynchronizeChanges(subtuple as ISynchronizationChanges);
                    property.SetValue(tuple.Local, subtuple!.Local);
                    property.SetValue(tuple.Synchronized, subtuple.Synchronized);
                    property.SetValue(tuple.Remote, subtuple.Remote);
                    continue;
                }

                var synchronizedValue = property.GetValue(tuple.Synchronized);
                var localValue = property.GetValue(tuple.Local);
                var remoteValue = property.GetValue(tuple.Remote);

                var localSynchronizedAndNotChanged = Equals(localValue, remoteValue) || Equals(synchronizedValue, remoteValue);
                var localNotChanged = Equals(synchronizedValue, localValue);
                var localMoreRelevant = localUpdatedAt > remoteUpdatedAt;

                var value = localSynchronizedAndNotChanged ? localValue
                    : localNotChanged                      ? remoteValue
                    : localMoreRelevant                    ? localValue
                                                             : remoteValue;

                UpdateValue(tuple, property, (localValue, synchronizedValue, remoteValue), value);
            }
        }

        private static void LinkEntities<T>(this SynchronizingTuple<T> tuple)
            where T : class, ISynchronizable<T>, new()
        {
            tuple.Synchronized.IsSynchronized = true;
            tuple.Local.ExternalID = tuple.Synchronized.ExternalID = tuple.ExternalID;
            tuple.Local.SynchronizationMate = tuple.Synchronized;
        }

        private static bool NeedMergeSubtype<T>(SynchronizingTuple<T> tuple, PropertyInfo property)
            => property.PropertyType.GetCustomAttribute(typeof(MergeContractAttribute)) != null &&
                (property.GetValue(tuple.Local) != null || property.GetValue(tuple.Remote) != null);
    }
}
