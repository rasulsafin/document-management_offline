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
        protected readonly IMapper mapper;

        protected ASynchronizationStrategy(IMapper mapper)
            => this.mapper = mapper;

        public async Task<List<SynchronizingResult>> Synchronize(SynchronizingData data,
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

            var results = new List<SynchronizingResult>();

            foreach (var tuple in tuples)
            {
                var action = tuple.DetermineAction();

                switch (action)
                {
                    case SynchronizingAction.Nothing:
                        await NothingAction(tuple, data, connectionContext, parent);
                        break;
                    case SynchronizingAction.Merge:
                        results.AddIsNotNull(await Merge(tuple, data, connectionContext, parent));
                        break;
                    case SynchronizingAction.AddToLocal:
                        results.AddIsNotNull(await AddToLocal(tuple, data, connectionContext, parent));
                        break;
                    case SynchronizingAction.AddToRemote:
                        results.AddIsNotNull(await AddToRemote(tuple, data, connectionContext, parent));
                        break;
                    case SynchronizingAction.RemoveFromLocal:
                        results.AddIsNotNull(await RemoveFromLocal(tuple, data, connectionContext, parent));
                        break;
                    case SynchronizingAction.RemoveFromRemote:
                        results.AddIsNotNull(await RemoveFromRemote(tuple, data, connectionContext, parent));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), "Invalid action");
                }
            }

            return results;
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
            object parent) => Task.CompletedTask;

        protected virtual async Task<SynchronizingResult> Merge(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                await UpdateRemote(tuple, GetSynchronizer(connectionContext).Update);
                UpdateDB(GetDBSet(data.Context), tuple);

                return null;
            }
            catch (Exception e)
            {
                return new SynchronizingResult()
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
            object parent)
        {
            try
            {
                UpdateDB(GetDBSet(data.Context), tuple);
                return null;
            }
            catch (Exception e)
            {
                return Task.FromResult(new SynchronizingResult()
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
            object parent)
        {
            try
            {
                await UpdateRemote(tuple, GetSynchronizer(connectionContext).Add);
                UpdateDB(GetDBSet(data.Context), tuple);
                return null;
            }
            catch (Exception e)
            {
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
            object parent)
        {
            try
            {
                RemoveFromDB(tuple, data);
                return null;
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
            object parent)
        {
            try
            {
                var project = mapper.Map<TDto>(tuple.Remote);
                await GetSynchronizer(connectionContext).Remove(project);
                RemoveFromDB(tuple, data);
                return null;
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
