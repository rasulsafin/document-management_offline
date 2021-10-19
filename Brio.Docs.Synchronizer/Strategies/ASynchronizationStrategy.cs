using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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

namespace Brio.Docs.Synchronization.Strategies
{
    internal abstract class ASynchronizationStrategy<TDB, TDto> : ISynchronizationStrategy<TDB, TDto>
        where TDB : class, ISynchronizable<TDB>, new()
    {
        protected readonly IMapper mapper;
        protected readonly DMContext context;
        private readonly bool needSaveOnEachTuple = false;
        private readonly ILogger<ASynchronizationStrategy<TDB, TDto>> logger;

        protected ASynchronizationStrategy(
            DMContext context,
            IMapper mapper,
            ILogger<ASynchronizationStrategy<TDB, TDto>> logger,
            bool needSaveOnEachTuple = true)
        {
            this.context = context;
            this.mapper = mapper;
            this.needSaveOnEachTuple = needSaveOnEachTuple;
            this.logger = logger;
            logger.LogTrace("ASynchronizationStrategy created");
        }

        public async Task<List<SynchronizingResult>> Synchronize(SynchronizingData data,
            IConnectionContext connectionContext,
            IEnumerable<TDB> remoteCollection,
            CancellationToken token,
            Expression<Func<TDB, bool>> dbFilter = null,
            Func<TDB, bool> remoteFilter = null,
            object parent = null,
            IProgress<double> progress = null)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Synchronize started");

            progress?.Report(0.0);
            var defaultFiler = GetDefaultFilter(data);
            var list = Include(GetDBSet(context))
               .Where(defaultFiler);

            if (dbFilter != null)
                list = list.Where(dbFilter);

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                Order(list),
                Order(remoteFilter == null ? remoteCollection : remoteCollection.Where(remoteFilter)),
                IsEntitiesEquals);
            logger.LogDebug("{@Count} tuples created", tuples.Count);

            var results = new List<SynchronizingResult>();
            var i = 0;

            foreach (var tuple in tuples)
            {
                logger.LogTrace("Tuple {ID}", tuple.ExternalID);
                token.ThrowIfCancellationRequested();

                var action = tuple.DetermineAction();
                logger.LogDebug("Tuple {ID} must {@Action}", tuple.ExternalID, action);

                try
                {
                    switch (action)
                    {
                        case SynchronizingAction.Nothing:
                            await NothingAction(tuple, data, connectionContext, parent);
                            break;
                        case SynchronizingAction.Merge:
                            results.AddIsNotNull(await Merge(tuple, data, connectionContext, parent, token));
                            break;
                        case SynchronizingAction.AddToLocal:
                            results.AddIsNotNull(await AddToLocal(tuple, data, connectionContext, parent, token));
                            break;
                        case SynchronizingAction.AddToRemote:
                            results.AddIsNotNull(await AddToRemote(tuple, data, connectionContext, parent, token));
                            break;
                        case SynchronizingAction.RemoveFromLocal:
                            results.AddIsNotNull(await RemoveFromLocal(tuple, data, connectionContext, parent, token));
                            break;
                        case SynchronizingAction.RemoveFromRemote:
                            results.AddIsNotNull(await RemoveFromRemote(tuple, data, connectionContext, parent, token));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(action), "Invalid action");
                    }

                    if (needSaveOnEachTuple)
                        await SaveDb(data);
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

        public virtual IReadOnlyCollection<TDB> Map(IReadOnlyCollection<TDto> externalDtos)
        {
            logger.LogTrace("Map started for externalDtos: {@Dtos}", externalDtos);
            return mapper.Map<IReadOnlyCollection<TDB>>(externalDtos);
        }

        protected abstract DbSet<TDB> GetDBSet(DMContext context);

        protected abstract ISynchronizer<TDto> GetSynchronizer(IConnectionContext context);

        protected abstract Expression<Func<TDB, bool>> GetDefaultFilter(SynchronizingData data);

        protected virtual IIncludableQueryable<TDB, TDB> Include(IQueryable<TDB> set)
            => set.Include(x => x.SynchronizationMate);

        protected virtual IEnumerable<TDB> Order(IEnumerable<TDB> list)
            => list;

        protected virtual bool IsEntitiesEquals(TDB element, SynchronizingTuple<TDB> tuple)
        {
            var hasID = element.ID > 0;
            var isLocalsMate = tuple.Local != null && element.ID == tuple.Local.SynchronizationMateID;
            var isSynchronizedsMate = tuple.Synchronized != null &&
                element.SynchronizationMateID == tuple.Synchronized.ID;
            var externalIDEquals = element.ExternalID != null && element.ExternalID == tuple.ExternalID;
            return (hasID && (isLocalsMate || isSynchronizedsMate)) || externalIDEquals;
        }

        protected virtual Task NothingAction(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            logger.LogStartAction(tuple, data, parent);
            return Task.CompletedTask;
        }

        protected virtual async Task<SynchronizingResult> Merge(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                token.ThrowIfCancellationRequested();

                await UpdateRemote(tuple, GetSynchronizer(connectionContext).Update);
                logger.LogTrace("Remote updated");
                UpdateDB(GetDBSet(context), tuple);
                logger.LogTrace("DB entities updated");

                return null;
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.Merge, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        protected virtual Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                token.ThrowIfCancellationRequested();

                UpdateDB(GetDBSet(context), tuple);
                logger.LogTrace("DB entities added");
                return Task.FromResult<SynchronizingResult>(null);
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToLocal, e, tuple);
                return Task.FromResult(new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                });
            }
        }

        protected virtual async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                token.ThrowIfCancellationRequested();

                await UpdateRemote(tuple, GetSynchronizer(connectionContext).Add);
                logger.LogTrace("Added to remote");
                UpdateDB(GetDBSet(context), tuple);
                logger.LogTrace("DB entities updated");
                return null;
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToRemote, e, tuple);
                tuple.Local.SynchronizationMate = null;
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        protected virtual Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                token.ThrowIfCancellationRequested();

                RemoveFromDB(tuple, data);
                logger.LogTrace("DB entities removed");
                return Task.FromResult<SynchronizingResult>(null);
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromLocal, e, tuple);
                return Task.FromResult(new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                });
            }
        }

        protected virtual async Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                token.ThrowIfCancellationRequested();

                await RemoveFromRemote(tuple, GetSynchronizer(connectionContext).Remove);
                logger.LogTrace("Removed from remote");
                RemoveFromDB(tuple, data);
                logger.LogTrace("DB entities removed");
                return null;
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromRemote, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        protected async Task SaveDb(SynchronizingData data)
        {
            if (data.Date == default)
                await context.SaveChangesAsync();
            else
                await context.SynchronizationSaveAsync(data.Date);
            logger.LogTrace("DB updated");
        }

        private void RemoveFromDB(SynchronizingTuple<TDB> tuple, SynchronizingData data)
        {
            logger.LogDebug("RemoveFromDB started with tuple {@Tuple}", tuple);
            if (tuple.Local != null)
            {
                GetDBSet(context).Remove(tuple.Local);
                logger.LogInformation("Removed {@ID}", tuple.Local.ID);
            }

            if (tuple.Synchronized != null)
            {
                GetDBSet(context).Remove(tuple.Synchronized);
                logger.LogDebug("Removed {@ID}", tuple.Synchronized.ID);
            }
        }

        private async Task RemoveFromRemote(SynchronizingTuple<TDB> tuple, Func<TDto, Task<TDto>> remoteFunc)
        {
            var dto = mapper.Map<TDto>(tuple.Remote);
            logger.LogDebug("Created dto: {@Dto}", dto);
            await remoteFunc(dto);
            logger.LogInformation("Removed {ID}", tuple.ExternalID);
        }

        private void UpdateDB(DbSet<TDB> set, SynchronizingTuple<TDB> tuple)
        {
            logger.LogBeforeMerge(tuple);
            tuple.Merge();
            logger.LogAfterMerge(tuple);

            if (tuple.Synchronized.ID == 0)
            {
                set.Add(tuple.Synchronized);
                logger.LogDebug("Added {ID} to DB", tuple.Synchronized.ExternalID);
            }
            else if (!tuple.SynchronizedChanged)
            {
                set.Update(tuple.Synchronized);
                logger.LogDebug("Updated {ID} ({ExternalID})", tuple.Synchronized.ID, tuple.ExternalID);
            }

            if (tuple.Local.ID == 0)
            {
                set.Add(tuple.Local);
                logger.LogInformation("Added {ID} to local", tuple.Local.ExternalID);
            }
            else if (!tuple.RemoteChanged)
            {
                set.Update(tuple.Local);
                logger.LogInformation("Updated {ID} ({ExternalID})", tuple.Local.ID, tuple.ExternalID);
            }
        }

        private async Task UpdateRemote(
            SynchronizingTuple<TDB> tuple,
            Func<TDto, Task<TDto>> remoteFunc)
        {
            logger.LogBeforeMerge(tuple);
            tuple.Merge();
            logger.LogAfterMerge(tuple);
            if (!tuple.RemoteChanged)
                return;

            var result = await remoteFunc(mapper.Map<TDto>(tuple.Remote));
            logger.LogDebug("Remote return {@Data}", result);
            tuple.Remote = mapper.Map<TDB>(result);
            logger.LogInformation("Put {ID} to remote", tuple.ExternalID);
        }
    }
}
