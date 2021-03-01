using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Synchronizer.Extensions;
using MRS.DocumentManagement.Synchronizer.Models;
using MRS.DocumentManagement.Synchronizer.Strategies;
using MRS.DocumentManagement.Synchronizer.Utils;

namespace MRS.DocumentManagement.Synchronizer.Strategies
{
    internal abstract class ASynchronizationStrategy<TDB, TDto>
        where TDB : class, ISynchronizable<TDB>, new()
    {
        private readonly IMapper mapper;

        public ASynchronizationStrategy(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public async Task Synchronize(SynchronizingData data,
            AConnectionContext connectionContext,
            Predicate<TDB> filter = null)
        {
            var tuples = TuplesUtils.CreateSynchronizingTuples(
                GetDBSet(data.Context).Where(x => DefaultFilter(data, x) && (filter == null || filter(x))),
                mapper.Map<IReadOnlyCollection<TDB>>(await connectionContext.Projects),
                IsEntitiesEquals);

            foreach (var tuple in tuples)
            {
                var action = tuple.DetermineAction();

                switch (action)
                {
                    case SynchronizingAction.Nothing:
                        await NothingAction(tuple, data, connectionContext);
                        break;
                    case SynchronizingAction.Merge:
                        await Merge(tuple, data, connectionContext);
                        break;
                    case SynchronizingAction.AddToLocal:
                        await AddToLocal(tuple, data, connectionContext);
                        break;
                    case SynchronizingAction.AddToRemote:
                        await AddToRemote(tuple, data, connectionContext);
                        break;
                    case SynchronizingAction.RemoveFromLocal:
                        await RemoveFromLocal(tuple, data, connectionContext);
                        break;
                    case SynchronizingAction.RemoveFromRemote:
                        await RemoveFromRemote(tuple, data, connectionContext);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), "Invalid action");
                }
            }
        }

        protected abstract DbSet<TDB> GetDBSet(DMContext context);

        protected abstract ISynchronizer<TDto> GetSynchronizer(AConnectionContext context);

        protected abstract bool DefaultFilter(SynchronizingData data, TDB item);

        protected virtual bool IsEntitiesEquals(TDB element, SynchronizingTuple<TDB> tuple)
            => (element.ID > 0 && element.ID == (int)tuple.GetPropertyValue(nameof(ISynchronizable<TDB>.ID))) ||
                (element.ExternalID != null && element.ExternalID == tuple.ExternalID);

        protected virtual Task NothingAction(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task Merge(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext)
        {
            await UpdateRemote(tuple, GetSynchronizer(connectionContext).Update);
            UpdateDB(GetDBSet(data.Context), tuple);
        }

        protected virtual Task AddToLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext)
        {
            UpdateDB(GetDBSet(data.Context), tuple);
            return Task.CompletedTask;
        }

        protected virtual async Task AddToRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext)
        {
            await UpdateRemote(tuple, GetSynchronizer(connectionContext).Add);
            UpdateDB(GetDBSet(data.Context), tuple);
        }

        protected virtual Task RemoveFromLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext)
        {
            RemoveFromDB(tuple, data);
            return Task.CompletedTask;
        }

        protected virtual async Task RemoveFromRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext)
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

            if (tuple.Local.ID > 0)
                set.Update(tuple.Local);
            else
                set.Add(tuple.Local);

            if (tuple.Synchronized.ID > 0)
                set.Update(tuple.Synchronized);
            else
                set.Add(tuple.Synchronized);
        }

        private async Task UpdateRemote(
            SynchronizingTuple<TDB> tuple,
            Func<TDto, Task<TDto>> remoteFunc)
        {
            tuple.Merge();
            var result = await remoteFunc(mapper.Map<TDto>(tuple.Remote));
            tuple.Remote = mapper.Map<TDB>(result);
        }
    }
}
