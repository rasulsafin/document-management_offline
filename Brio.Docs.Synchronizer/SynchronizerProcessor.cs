using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization
{
    internal class SynchronizerProcessor : ISynchronizerProcessor
    {
        private readonly DMContext context;
        private readonly ILogger<SynchronizerProcessor> logger;

        public SynchronizerProcessor(
            DMContext context,
            ILogger<SynchronizerProcessor> logger)
        {
            this.context = context;
            this.logger = logger;
            logger.LogTrace("ASynchronizationStrategy created");
        }

        public async Task<List<SynchronizingResult>> Synchronize<TDB, TDto>(
            ISynchronizationStrategy<TDB> strategy,
            SynchronizingData data,
            IEnumerable<TDB> remoteCollection,
            Expression<Func<TDB, bool>> filter,
            CancellationToken token,
            IProgress<double> progress = null)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Synchronize started");

            progress?.Report(0.0);

            var defaultFiler = strategy.GetFilter(data);

            var local = strategy.Order(
                context.Set<TDB>()
                   .Unsynchronized()
                   .Include(x => x.SynchronizationMate)
                   .Where(defaultFiler)
                   .Where(filter));
            var synchronized = strategy.Order(
                context.Set<TDB>()
                   .Synchronized()
                   .Include(x => x.SynchronizationMate)
                   .Where(defaultFiler)
                   .Where(filter));
            var remote = strategy.Order(
                remoteCollection
                   .Where(filter.Compile()));

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                local,
                synchronized,
                remote,
                (element, tuple) => tuple.DoesNeed(element));

            logger.LogDebug("{@Count} tuples created", tuples.Count);

            var results = new List<SynchronizingResult>();
            var i = 0;
            DBContextUtilities.ReloadContext(context);

            foreach (var tuple in tuples)
            {
                context.Attach(data.User);
                foreach (var db in tuple.AsEnumerable().Where(x => x != null && x.ID != 0))
                    context.Attach(db);
                logger.LogTrace("Tuple {ID}", tuple.ExternalID);
                token.ThrowIfCancellationRequested();

                var action = tuple.DetermineAction();
                logger.LogDebug("Tuple {ID} must {@Action}", tuple.ExternalID, action);

                try
                {
                    SynchronizationFunc<TDB> func = action switch
                    {
                        SynchronizingAction.Nothing => (_, _, _) => Task.FromResult(default(SynchronizingResult)),
                        SynchronizingAction.Merge => strategy.Merge,
                        SynchronizingAction.AddToLocal => strategy.AddToLocal,
                        SynchronizingAction.AddToRemote => strategy.AddToRemote,
                        SynchronizingAction.RemoveFromLocal => strategy.RemoveFromLocal,
                        SynchronizingAction.RemoveFromRemote => strategy.RemoveFromRemote,
                        _ => throw new ArgumentOutOfRangeException(nameof(action), "Invalid action")
                    };

                    var synchronizingResult = await func.Invoke(tuple, data, token).ConfigureAwait(false);
                    results.AddIsNotNull(synchronizingResult);

                    await SaveDb(data).ConfigureAwait(false);
                    DBContextUtilities.ReloadContext(context);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Synchronization failed");
                    DBContextUtilities.ReloadContext(context);

                    var isRemote = action == SynchronizingAction.AddToLocal;
                    results.Add(
                        new SynchronizingResult
                        {
                            Exception = e,
                            Object = isRemote ? tuple.Remote : tuple.Local,
                            ObjectType = isRemote ? ObjectType.Remote : ObjectType.Local,
                        });
                }

                progress?.Report(++i / (double)tuples.Count);
            }

            progress?.Report(1.0);
            return results;
        }

        protected async Task SaveDb(SynchronizingData data)
        {
            if (data.Date == default)
                await context.SaveChangesAsync().ConfigureAwait(false);
            else
                await context.SynchronizationSaveAsync(data.Date).ConfigureAwait(false);
            logger.LogTrace("DB updated");
        }
    }
}
