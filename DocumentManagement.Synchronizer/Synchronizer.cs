using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Synchronizer.Extensions;
using MRS.DocumentManagement.Synchronizer.Models;

namespace MRS.DocumentManagement.Synchronizer
{
    public class Synchronizer
    {
        private readonly IMapper mapper;

        public Synchronizer(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public async Task<SynchronizingResult> Synchronize(
                SynchronizingData data,
                IConnection connection)
        {
            var context = connection.GetContext();
            await Synchronize(context.ProjectsSynchronizer, data.Projects, await context.Projects);
            await Synchronize(context.ObjectivesSynchronizer, data.Objectives, await context.Objectives);
        }

        private static List<SynchronizingTuple<T>> CreateSynchronizingTuples<T>(
                IEnumerable<T> dbList,
                IEnumerable<T> remoteList)
                where T : ISynchronizable<T>
        {
            var result = new List<SynchronizingTuple<T>>();

            void AddToList(IEnumerable<T> list, Action<SynchronizingTuple<T>, T> setUnsynchronized)
            {
                foreach (var item in list)
                {
                    if (string.IsNullOrEmpty(item.ExternalID))
                    {
                        result.Add(new SynchronizingTuple<T>(local: item));
                        continue;
                    }

                    var containsItem = result.FirstOrDefault(x => x.ExternalID == item.ExternalID);
                    if (containsItem == null)
                        result.Add(containsItem = new SynchronizingTuple<T>(item.ExternalID));
                    if (item.IsSynchronized)
                        containsItem.Synchronized = item;
                    else
                        setUnsynchronized(containsItem, item);
                }
            }

            AddToList(dbList, (tuple, item) => tuple.Local = item);
            AddToList(remoteList, (tuple, item) => tuple.Remote = item);

            return result;
        }

        private async Task Synchronize<TDB, TDto>(ISynchronizer<TDto> synchronizer, DbSet<TDB> dbSet, IReadOnlyCollection<TDto> remote)
                where TDB : class, ISynchronizable<TDB>, new()
        {
            var tuples = CreateSynchronizingTuples(
                    dbSet,
                    mapper.Map<IEnumerable<TDB>>(remote));

            void UpdateDB(SynchronizingTuple<TDB> tuple)
            {
                tuple.Merge();
                dbSet.Update(tuple.Local);
                dbSet.Update(tuple.Synchronized);
            }

            async Task UpdateRemote(
                    SynchronizingTuple<TDB> tuple,
                    Func<TDto, Task<TDto>> remoteFunc)
            {
                tuple.Merge();
                var project = mapper.Map<TDto>(tuple.Remote);
                var result = await remoteFunc(project);
                tuple.Remote = mapper.Map<TDB>(result);
            }

            foreach (var tuple in tuples)
            {
                var action = tuple.DetermineAction();

                switch (action)
                {
                    case SynchronizingAction.Nothing:
                        break;
                    case SynchronizingAction.Merge:
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
                        if (tuple.Remote != null)
                            dbSet.Remove(tuple.Remote);
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
    }
}
