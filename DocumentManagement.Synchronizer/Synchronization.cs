using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Synchronizer.Extensions;
using MRS.DocumentManagement.Synchronizer.Models;

namespace MRS.DocumentManagement.Synchronizer
{
    internal class Synchronization<TDB, TDto>
            where TDB : class, ISynchronizable<TDB>, new()
    {
        private readonly IMapper mapper;
        private readonly ISynchronizer<TDto> synchronizer;
        private readonly DbSet<TDB> dbSet;
        private readonly Predicate<TDB> filter;
        private readonly IReadOnlyCollection<TDto> remote;
        private readonly bool needMerge;

        public Synchronization(
                IMapper mapper,
                ISynchronizer<TDto> synchronizer,
                DbSet<TDB> dbSet,
                Predicate<TDB> filter,
                IReadOnlyCollection<TDto> remote,
                bool needMerge)
        {
            this.mapper = mapper;
            this.synchronizer = synchronizer;
            this.dbSet = dbSet;
            this.filter = filter;
            this.remote = remote;
            this.needMerge = needMerge;
        }

        public async Task Synchronize()
        {
            var tuples = CreateSynchronizingTuples(
                    dbSet.Where(x => filter(x)),
                    mapper.Map<IReadOnlyCollection<TDB>>(remote));

            foreach (var tuple in tuples)
            {
                var action = tuple.DetermineAction();

                switch (action)
                {
                    case SynchronizingAction.Nothing:
                        break;
                    case SynchronizingAction.Merge:
                        if (!needMerge)
                            break;
                        await UpdateRemote(tuple, synchronizer.Update);
                        UpdateDB(tuple);
                        break;
                    case SynchronizingAction.AddToLocal:
                        UpdateDB(tuple);
                        break;
                    case SynchronizingAction.AddToRemote:
                        await UpdateRemote(tuple, synchronizer.Add);
                        UpdateDB(tuple);
                        break;
                    case SynchronizingAction.RemoveFromLocal:
                        if (tuple.Local != null)
                            dbSet.Remove(tuple.Local);
                        if (tuple.Synchronized != null)
                            dbSet.Remove(tuple.Synchronized);
                        break;
                    case SynchronizingAction.RemoveFromRemote:
                        var project = mapper.Map<TDto>(tuple.Remote);
                        await synchronizer.Remove(project);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), "Invalid action");
                }
            }
        }

        private static List<SynchronizingTuple<T>> CreateSynchronizingTuples<T>(
                IEnumerable<T> dbList,
                IEnumerable<T> remoteList)
                where T : ISynchronizable<T>
        {
            var result = new List<SynchronizingTuple<T>>();

            void AddToList(IEnumerable<T> list, Action<SynchronizingTuple<T>, T> setUnsynchronized)
            {
                foreach (var element in list)
                {
                    if (string.IsNullOrEmpty(element.ExternalID))
                    {
                        result.Add(new SynchronizingTuple<T>(local: element));
                        continue;
                    }

                    bool IsEqualsEntities(SynchronizingTuple<T> x)
                        => x.ExternalID == element.ExternalID
                           || (element is Item item
                               && ((x.Local is Item local && item.Name == local.Name)
                                   || (x.Remote is Item remote && item.Name == remote.Name)
                                   || (x.Synchronized is Item synchronized && item.Name == synchronized.Name)));

                    var containsItem = result.FirstOrDefault(IsEqualsEntities);
                    if (containsItem == null)
                        result.Add(containsItem = new SynchronizingTuple<T>(element.ExternalID));
                    if (element.IsSynchronized)
                        containsItem.Synchronized = element;
                    else
                        setUnsynchronized(containsItem, element);
                }
            }

            AddToList(dbList, (tuple, item) => tuple.Local = item);
            AddToList(remoteList, (tuple, item) => tuple.Remote = item);

            return result;
        }

        private void UpdateDB(SynchronizingTuple<TDB> tuple)
        {
            tuple.Merge();

            if (tuple.Local.ID > 0)
                dbSet.Update(tuple.Local);
            else
                dbSet.Add(tuple.Local);

            if (tuple.Synchronized.ID > 0)
                dbSet.Update(tuple.Synchronized);
            else
                dbSet.Add(tuple.Synchronized);
        }

        private async Task UpdateRemote(SynchronizingTuple<TDB> tuple, Func<TDto, Task<TDto>> remoteFunc)
        {
            tuple.Merge();
            var project = mapper.Map<TDto>(tuple.Remote);
            var result = await remoteFunc(project);
            tuple.Remote = mapper.Map<TDB>(result);
        }
    }
}
