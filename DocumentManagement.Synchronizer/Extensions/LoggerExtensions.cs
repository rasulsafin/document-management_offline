using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Extensions
{
    internal static class LoggerExtensions
    {
        internal static void LogStartAction<TDB>(
            this ILogger logger,
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            object parent,
            [CallerMemberName] string method = "")
            => logger.LogTrace(
                "{@Method} started with tuple:\r\n{@Tuple}\r\nparent:\r\n{@Parent}\r\ndata:\r\n{@SynchronizingData}",
                method,
                tuple,
                parent,
                data);

        internal static void LogBeforeMerge<TDB>(this ILogger logger, SynchronizingTuple<TDB> tuple)
            => logger.LogDebug("Tuple before merge: {@Tuple}", tuple);

        internal static void LogAfterMerge<TDB>(this ILogger logger, SynchronizingTuple<TDB> tuple)
            => logger.LogDebug("Tuple after merge: {@Tuple}", tuple);

        internal static void LogExceptionOnAction<TDB>(
            this ILogger logger,
            SynchronizingAction action,
            Exception exception,
            SynchronizingTuple<TDB> tuple)
        {
            switch (action)
            {
                case SynchronizingAction.Nothing:
                    logger.LogWarning(
                        exception,
                        "Nothing action failed on:\r\nlocal:{@Local}\r\nremote{@Remote}",
                        tuple.Local,
                        tuple.Remote);
                    break;
                case SynchronizingAction.Merge:
                    logger.LogWarning(
                        exception,
                        "Merging failed on:\r\nlocal:{@Local}\r\nremote{@Remote}",
                        tuple.Local,
                        tuple.Remote);
                    break;
                case SynchronizingAction.AddToLocal:
                    logger.LogWarning(exception, "Adding to local failed on: {@Object}", tuple.Remote);
                    break;
                case SynchronizingAction.AddToRemote:
                    logger.LogWarning(exception, "Adding to remote failed on: {@Object}", tuple.Local);
                    break;
                case SynchronizingAction.RemoveFromLocal:
                    logger.LogWarning(exception, "Removing from local failed on: {@Object}", tuple.Local);
                    break;
                case SynchronizingAction.RemoveFromRemote:
                    logger.LogWarning(exception, "Removing from remote failed on: {@Object}", tuple.Synchronized);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        internal static void LogCanceled(this ILogger logger)
            => logger.LogDebug("Operation canceled");
    }
}
