using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization
{
    internal class SynchronizerProcessor : ISynchronizerProcessor
    {
        private readonly DMContext context;
        private readonly ILogger<SynchronizerProcessor> logger;
        private readonly bool needSaveOnEachTuple = false;

        public SynchronizerProcessor(
            DMContext context,
            ILogger<SynchronizerProcessor> logger)
        {
            this.context = context;
            this.logger = logger;
            logger.LogTrace("ASynchronizationStrategy created");
        }

        public async Task<List<SynchronizingResult>> Synchronize<TDB, TDto>(
            ISynchronizationStrategy<TDB, TDto> strategy,
            SynchronizingData data,
            IConnectionContext connectionContext,
            IEnumerable<TDB> remoteCollection,
            CancellationToken token,
            Expression<Func<TDB, bool>> defaultFiler,
            Expression<Func<TDB, bool>> dbFilter = null,
            Func<TDB, bool> remoteFilter = null,
            IProgress<double> progress = null)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Synchronize started");

            progress?.Report(0.0);
            var list = Include(context.Set<TDB>()).Where(defaultFiler);

            if (dbFilter != null)
                list = list.Where(dbFilter);

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                strategy.Order(list),
                strategy.Order(remoteFilter == null ? remoteCollection : remoteCollection.Where(remoteFilter)),
                IsEntitiesEquals);
            logger.LogDebug("{@Count} tuples created", tuples.Count);

            var results = new List<SynchronizingResult>();
            var i = 0;

            foreach (var tuple in tuples)
            {
                foreach (var db in tuple.AsEnumerable().Where(x => x != null && x.ID != 0))
                    context.Attach(db);
                logger.LogTrace("Tuple {ID}", tuple.ExternalID);
                token.ThrowIfCancellationRequested();

                var action = tuple.DetermineAction();
                logger.LogDebug("Tuple {ID} must {@Action}", tuple.ExternalID, action);

                try
                {
                    switch (action)
                    {
                        case SynchronizingAction.Nothing:
                            break;
                        case SynchronizingAction.Merge:
                            results.AddIsNotNull(
                                await strategy.Merge(tuple, data, connectionContext, token).ConfigureAwait(false));
                            break;
                        case SynchronizingAction.AddToLocal:
                            results.AddIsNotNull(
                                await strategy.AddToLocal(tuple, data, token).ConfigureAwait(false));
                            break;
                        case SynchronizingAction.AddToRemote:
                            results.AddIsNotNull(
                                await strategy.AddToRemote(tuple, data, connectionContext, token)
                                   .ConfigureAwait(false));
                            break;
                        case SynchronizingAction.RemoveFromLocal:
                            results.AddIsNotNull(
                                await strategy.RemoveFromLocal(tuple, token)
                                   .ConfigureAwait(false));
                            break;
                        case SynchronizingAction.RemoveFromRemote:
                            results.AddIsNotNull(
                                await strategy.RemoveFromRemote(tuple, connectionContext, token)
                                   .ConfigureAwait(false));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(action), "Invalid action");
                    }

                    if (needSaveOnEachTuple)
                    {
                        await SaveDb(data).ConfigureAwait(false);
                        DBContextUtilities.ReloadContext(context);
                    }
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

        protected virtual IIncludableQueryable<TDB, TDB> Include<TDB>(IQueryable<TDB> set)
            where TDB : class, ISynchronizable<TDB>, new()
            => set.Include(x => x.SynchronizationMate);

        protected virtual bool IsEntitiesEquals<TDB>(TDB element, SynchronizingTuple<TDB> tuple)
            where TDB : class, ISynchronizable<TDB>, new()
            => tuple.DoesNeed(element);

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
