using System;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Extensions
{
    internal static class SynchronizationTupleExtensions
    {
        public static bool All<T>(this SynchronizingTuple<T> tuple, Predicate<T> predicate)
            => predicate(tuple.Local) && predicate(tuple.Synchronized) && predicate(tuple.Remote);

        public static async Task<bool> AllAsync<T>(this SynchronizingTuple<T> tuple, Func<T, Task<bool>> predicate)
            => await predicate(tuple.Local).ConfigureAwait(false) &&
                await predicate(tuple.Synchronized).ConfigureAwait(false) &&
                await predicate(tuple.Remote).ConfigureAwait(false);

        public static async ValueTask<bool> AllAsync<T>(this SynchronizingTuple<T> tuple, Func<T, ValueTask<bool>> predicate)
            => await predicate(tuple.Local).ConfigureAwait(false) &&
                await predicate(tuple.Synchronized).ConfigureAwait(false) &&
                await predicate(tuple.Remote).ConfigureAwait(false);

        public static bool Any<T>(this SynchronizingTuple<T> tuple, Predicate<T> predicate)
            => predicate(tuple.Local) || predicate(tuple.Synchronized) || predicate(tuple.Remote);

        public static async Task<bool> AnyAsync<T>(this SynchronizingTuple<T> tuple, Func<T, Task<bool>> predicate)
            => await predicate(tuple.Local).ConfigureAwait(false) ||
                await predicate(tuple.Synchronized).ConfigureAwait(false) ||
                await predicate(tuple.Remote).ConfigureAwait(false);

        public static async ValueTask<bool> AnyAsync<T>(this SynchronizingTuple<T> tuple, Func<T, ValueTask<bool>> predicate)
            => await predicate(tuple.Local).ConfigureAwait(false) ||
                await predicate(tuple.Synchronized).ConfigureAwait(false) ||
                await predicate(tuple.Remote).ConfigureAwait(false);

        public static bool DoesNeed<T>(this SynchronizingTuple<T> tuple, T element)
            where T : ISynchronizable<T>
        {
            var hasID = element.ID > 0;
            var isLocalsMate = tuple.Local != null && element.ID == tuple.Local.SynchronizationMateID;
            var isSynchronizedsMate = tuple.Synchronized != null &&
                element.SynchronizationMateID == tuple.Synchronized.ID;
            var externalIDEquals = element.ExternalID != null && element.ExternalID == tuple.ExternalID;
            return (hasID && (isLocalsMate || isSynchronizedsMate)) || externalIDEquals;
        }

        public static void ForEach<T>(this SynchronizingTuple<T> tuple, Action<T> actionAsync)
        {
            actionAsync(tuple.Local);
            actionAsync(tuple.Synchronized);
            actionAsync(tuple.Remote);
        }

        public static async Task ForEachAsync<T>(this SynchronizingTuple<T> tuple, Func<T, Task> actionAsync)
        {
            await actionAsync(tuple.Local).ConfigureAwait(false);
            await actionAsync(tuple.Synchronized).ConfigureAwait(false);
            await actionAsync(tuple.Remote).ConfigureAwait(false);
        }

        public static async ValueTask ForEachAsync<T>(this SynchronizingTuple<T> tuple, Func<T, ValueTask> actionAsync)
        {
            await actionAsync(tuple.Local).ConfigureAwait(false);
            await actionAsync(tuple.Synchronized).ConfigureAwait(false);
            await actionAsync(tuple.Remote).ConfigureAwait(false);
        }

        public static void ForEachChange<TParent, TChild>(
            this SynchronizingTuple<TParent> parentTuple,
            SynchronizingTuple<TChild> childTuple,
            Func<TParent, TChild, bool> actionAsync)
        {
            parentTuple.LocalChanged |= actionAsync(parentTuple.Local, childTuple.Local);
            parentTuple.SynchronizedChanged |= actionAsync(parentTuple.Synchronized, childTuple.Synchronized);
            parentTuple.RemoteChanged |= actionAsync(parentTuple.Remote, childTuple.Remote);
        }

        public static async Task ForEachChangeAsync<TParent, TChild>(
            this SynchronizingTuple<TParent> parentTuple,
            SynchronizingTuple<TChild> childTuple,
            Func<TParent, TChild, Task<bool>> actionAsync)
        {
            parentTuple.LocalChanged |= await actionAsync(parentTuple.Local, childTuple.Local).ConfigureAwait(false);
            parentTuple.SynchronizedChanged |= await actionAsync(parentTuple.Synchronized, childTuple.Synchronized)
               .ConfigureAwait(false);
            parentTuple.RemoteChanged |= await actionAsync(parentTuple.Remote, childTuple.Remote).ConfigureAwait(false);
        }

        public static async ValueTask ForEachChangeAsync<TParent, TChild>(
            this SynchronizingTuple<TParent> parentTuple,
            SynchronizingTuple<TChild> childTuple,
            Func<TParent, TChild, ValueTask<bool>> actionAsync)
        {
            parentTuple.LocalChanged |= await actionAsync(parentTuple.Local, childTuple.Local).ConfigureAwait(false);
            parentTuple.SynchronizedChanged |= await actionAsync(parentTuple.Synchronized, childTuple.Synchronized)
               .ConfigureAwait(false);
            parentTuple.RemoteChanged |= await actionAsync(parentTuple.Remote, childTuple.Remote).ConfigureAwait(false);
        }

        public static void RemoveWhere<T>(this SynchronizingTuple<T> tuple, Predicate<T> predicate)
        {
            if (predicate(tuple.Local))
                tuple.Local = default;
            if (predicate(tuple.Synchronized))
                tuple.Synchronized = default;
            if (predicate(tuple.Remote))
                tuple.Remote = default;
        }
    }
}
