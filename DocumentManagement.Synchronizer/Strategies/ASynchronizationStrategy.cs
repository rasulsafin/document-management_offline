using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Synchronization.Extensions;
using MRS.DocumentManagement.Synchronization.Interfaces;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Utils;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal abstract class ASynchronizationStrategy<TDB, TDto> : ISynchronizationStrategy<TDB, TDto>
        where TDB : class, ISynchronizable<TDB>, new()
    {
        protected readonly IMapper mapper;
        protected readonly DMContext context;
        private readonly bool needSaveOnEachTuple = false;

        protected ASynchronizationStrategy(DMContext context, IMapper mapper, bool needSaveOnEachTuple = true)
        {
            this.context = context;
            this.mapper = mapper;
            this.needSaveOnEachTuple = needSaveOnEachTuple;
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

            var results = new List<SynchronizingResult>();
            var i = 0;

            foreach (var tuple in tuples)
            {
                token.ThrowIfCancellationRequested();

                var action = tuple.DetermineAction();

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
                {
                    if (data.Date == default)
                        await context.SaveChangesAsync();
                    else
                        await context.SynchronizationSaveAsync(data.Date);
                }

                progress?.Report(++i / (double)tuples.Count);
            }

            progress?.Report(1.0);
            return results;
        }

        public virtual IReadOnlyCollection<TDB> Map(IReadOnlyCollection<TDto> externalDtos)
            => mapper.Map<IReadOnlyCollection<TDB>>(externalDtos);

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
            object parent) => Task.CompletedTask;

        protected virtual async Task<SynchronizingResult> Merge(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                await UpdateRemote(tuple, GetSynchronizer(connectionContext).Update);
                UpdateDB(GetDBSet(context), tuple);

                return null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
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
            try
            {
                token.ThrowIfCancellationRequested();

                UpdateDB(GetDBSet(context), tuple);
                return Task.FromResult<SynchronizingResult>(null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
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
            try
            {
                token.ThrowIfCancellationRequested();

                await UpdateRemote(tuple, GetSynchronizer(connectionContext).Add);
                UpdateDB(GetDBSet(context), tuple);
                return null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                tuple.Local.SynchronizationMate = null;
                return new SynchronizingResult()
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
            try
            {
                token.ThrowIfCancellationRequested();

                RemoveFromDB(tuple, data);
                return Task.FromResult<SynchronizingResult>(null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                return Task.FromResult(new SynchronizingResult()
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
            try
            {
                token.ThrowIfCancellationRequested();

                var project = mapper.Map<TDto>(tuple.Remote);
                await GetSynchronizer(connectionContext).Remove(project);
                RemoveFromDB(tuple, data);
                return null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                return new SynchronizingResult()
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        private void RemoveFromDB(SynchronizingTuple<TDB> tuple, SynchronizingData data)
        {
            if (tuple.Local != null)
                GetDBSet(context).Remove(tuple.Local);
            if (tuple.Synchronized != null)
                GetDBSet(context).Remove(tuple.Synchronized);
        }

        private void UpdateDB(DbSet<TDB> set, SynchronizingTuple<TDB> tuple)
        {
            tuple.Merge();

            if (tuple.Synchronized.ID == 0)
                set.Add(tuple.Synchronized);
            else if (!tuple.SynchronizedChanged)
                set.Update(tuple.Synchronized);

            if (tuple.Local.ID == 0)
                set.Add(tuple.Local);
            else if (!tuple.RemoteChanged)
                set.Update(tuple.Local);
        }

        private async Task UpdateRemote(
            SynchronizingTuple<TDB> tuple,
            Func<TDto, Task<TDto>> remoteFunc)
        {
            tuple.Merge();
            if (!tuple.RemoteChanged)
                return;

            var result = await remoteFunc(mapper.Map<TDto>(tuple.Remote));
            tuple.Remote = mapper.Map<TDB>(result);
        }
    }
}
