using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Synchronization.Extensions;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Utils;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal abstract class ASynchronizationStrategy<TDB, TDto>
        where TDB : class, ISynchronizable<TDB>, new()
    {
        private readonly IMapper mapper;

        protected ASynchronizationStrategy(IMapper mapper)
            => this.mapper = mapper;

        public async Task Synchronize(SynchronizingData data,
            IConnectionContext connectionContext,
            IEnumerable<TDB> remoteCollection,
            Expression<Func<TDB, bool>> filter = null,
            object parent = null)
        {
            var defaultFiler = GetDefaultFilter(data);
            var list = Include(GetDBSet(data.Context))
               .Where(defaultFiler);

            if (filter != null)
                list = list.Where(filter);

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                list,
                remoteCollection,
                IsEntitiesEquals);

            foreach (var tuple in tuples)
            {
                var action = tuple.DetermineAction();

                switch (action)
                {
                    case SynchronizingAction.Nothing:
                        await NothingAction(tuple, data, connectionContext, parent);
                        break;
                    case SynchronizingAction.Merge:
                        await Merge(tuple, data, connectionContext, parent);
                        break;
                    case SynchronizingAction.AddToLocal:
                        await AddToLocal(tuple, data, connectionContext, parent);
                        break;
                    case SynchronizingAction.AddToRemote:
                        await AddToRemote(tuple, data, connectionContext, parent);
                        break;
                    case SynchronizingAction.RemoveFromLocal:
                        await RemoveFromLocal(tuple, data, connectionContext, parent);
                        break;
                    case SynchronizingAction.RemoveFromRemote:
                        await RemoveFromRemote(tuple, data, connectionContext, parent);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), "Invalid action");
                }
            }
        }

        protected abstract DbSet<TDB> GetDBSet(DMContext context);

        protected abstract ISynchronizer<TDto> GetSynchronizer(IConnectionContext context);

        protected abstract Expression<Func<TDB, bool>> GetDefaultFilter(SynchronizingData data);

        protected virtual IIncludableQueryable<TDB, TDB> Include(IQueryable<TDB> set)
            => set.Include(x => x.SynchronizationMate);

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
            => Task.CompletedTask;

        protected virtual async Task Merge(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await UpdateRemote(tuple, GetSynchronizer(connectionContext).Update);
            UpdateDB(GetDBSet(data.Context), tuple);
        }

        protected virtual Task AddToLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            UpdateDB(GetDBSet(data.Context), tuple);
            return Task.CompletedTask;
        }

        protected virtual async Task AddToRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await UpdateRemote(tuple, GetSynchronizer(connectionContext).Add);
            UpdateDB(GetDBSet(data.Context), tuple);
        }

        protected virtual Task RemoveFromLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            RemoveFromDB(tuple, data);
            return Task.CompletedTask;
        }

        protected virtual async Task RemoveFromRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            var project = mapper.Map<TDto>(tuple.Remote);
            await GetSynchronizer(connectionContext).Remove(project);
            RemoveFromDB(tuple, data);
        }

        private void RemoveFromDB(SynchronizingTuple<TDB> tuple, SynchronizingData data)
        {
            if (tuple.Local != null)
                GetDBSet(data.Context).Remove(tuple.Local);
            if (tuple.Synchronized != null)
                GetDBSet(data.Context).Remove(tuple.Synchronized);
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
