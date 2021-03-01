using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronizer.Extensions;
using MRS.DocumentManagement.Synchronizer.Models;

namespace MRS.DocumentManagement.Synchronizer
{
    public class Synchronizer
    {
        private readonly IMapper mapper;

        public Synchronizer(IMapper mapper)
            => this.mapper = mapper;

        public async Task<SynchronizingResult> Synchronize(
                SynchronizingData data,
                IConnection connection,
                ConnectionInfoDto info)
        {
            var date = DateTime.UtcNow;
            var lastSynchronization = await data.Synchronizations.AnyAsync()
                    ? (await data.Synchronizations.LastAsync()).Date
                    : DateTime.MinValue;
            var context = await connection.GetContext(info, lastSynchronization);
            await Synchronize(
                    context.ProjectsSynchronizer,
                    context.ItemsSynchronizer,
                    data.Projects,
                    data.ProjectsFilter,
                    data.Items,
                    await context.Projects);
            await Synchronize(
                    context.ObjectivesSynchronizer,
                    context.ItemsSynchronizer,
                    data.Objectives,
                    data.ObjectivesFilter,
                    data.Items,
                    await context.Objectives);
            await data.Synchronizations.AddAsync(new Database.Models.Synchronization { Date = date });
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

        private async Task Synchronize<TDB, TDto>(
                ISynchronizer<TDto> synchronizer,
                ISynchronizer<ItemExternalDto> itemsSynchronizer,
                DbSet<TDB> dbSet,
                Predicate<TDB> filter,
                DbSet<Item> itemsSet,
                IReadOnlyCollection<TDto> remote)
                where TDB : class, ISynchronizable<TDB>, new()
        {
            var tuples = CreateSynchronizingTuples(
                    dbSet.Where(x => filter(x))
                            .Include(x => (x as Project).Items)
                            .ThenInclude(x => x.Project)
                            .ThenInclude(x => x.Objectives)
                            .ThenInclude(x => x.Items)
                            .ThenInclude(x => x.Item),
                    mapper.Map<IReadOnlyCollection<TDB>>(remote));

            foreach (var tuple in tuples)
            {
                var action = tuple.DetermineAction();

                switch (action)
                {
                    case SynchronizingAction.Nothing:
                        break;
                    case SynchronizingAction.Merge:
                        await SynchronizeItems(itemsSynchronizer, itemsSet, tuple);
                        await UpdateRemote<TDB, TDto>(tuple, synchronizer.Update);
                        UpdateDB(dbSet, tuple);
                        break;
                    case SynchronizingAction.AddToLocal:
                        UpdateDB(dbSet, tuple);
                        break;
                    case SynchronizingAction.AddToRemote:
                        await UpdateRemote<TDB, TDto>(tuple, synchronizer.Add);
                        UpdateDB(dbSet, tuple);
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

        private async Task SynchronizeItems<T>(
                ISynchronizer<ItemExternalDto> itemsSynchronizer,
                DbSet<Item> itemsSet,
                SynchronizingTuple<T> parentTuple)
                where T : class, ISynchronizable<T>, new()
        {
            var itemsTuples = CreateSynchronizingTuples(
                    // TODO: Do for objective too.
                    (parentTuple.Local as Project)?.Items,
                    (parentTuple.Remote as Project)?.Items);

            foreach (var tuple in itemsTuples)
            {
                var action = tuple.DetermineAction();

                switch (action)
                {
                    case SynchronizingAction.Nothing:
                    case SynchronizingAction.Merge:
                        break;
                    case SynchronizingAction.AddToLocal:
                        UpdateDB(itemsSet, tuple);
                        break;
                    case SynchronizingAction.AddToRemote:
                        var added = await itemsSynchronizer.Add(mapper.Map<ItemExternalDto>(tuple.Remote));
                        tuple.Remote = mapper.Map<Item>(added);

                        // TODO: tuple.Remote.FullPath = tuple.Local.FullPath.
                        UpdateDB(itemsSet, tuple);
                        break;
                    case SynchronizingAction.RemoveFromLocal:
                        UnlinkItemFromProject<T>(tuple, itemsSet);
                        break;
                    case SynchronizingAction.RemoveFromRemote:
                        var i = mapper.Map<ItemExternalDto>(tuple.Remote);
                        await itemsSynchronizer.Remove(i);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), "Invalid action");
                }
            }
        }

        private void UpdateDB<T>(DbSet<T> set, SynchronizingTuple<T> tuple)
                where T : class, ISynchronizable<T>, new()
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

        private async Task UpdateRemote<TDB, TDto>(SynchronizingTuple<TDB> tuple, Func<TDto, Task<TDto>> remoteFunc)
                where TDB : class, ISynchronizable<TDB>, new()
        {
            tuple.Merge();
            var project = mapper.Map<TDto>(tuple.Remote);
            var result = await remoteFunc(project);
            tuple.Remote = mapper.Map<TDB>(result);
        }

        private void UnlinkItemFromProject<T>(
                SynchronizingTuple<Item> tuple,
                DbSet<Item> items)
                where T : class, ISynchronizable<T>
        {
            tuple.Local.ProjectID = null;
            tuple.Synchronized.ProjectID = null;
            if (tuple.Local.ID > 0)
                items.Update(tuple.Local);
            if (tuple.Synchronized.ID > 0)
                items.Update(tuple.Synchronized);

            var project = tuple.Local.Project;
            if (tuple.Local != null && IsLinked(project, project.Objectives, tuple.Local))
                items.Remove(tuple.Local);
            project = tuple.Synchronized.Project;
            if (tuple.Synchronized != null && IsLinked(project, project.Objectives, tuple.Synchronized))
                items.Remove(tuple.Remote);
        }

        private async Task UnlinkItemFromObjective<T>(
                SynchronizingTuple<Item> tuple,
                Objective objective,
                DbSet<Item> items,
                DbSet<ObjectiveItem> bridge)
                where T : class, ISynchronizable<T>
        {
            if (tuple.Local.ID > 0)
            {
                var found = await bridge
                        .FirstOrDefaultAsync(x => x.ItemID == tuple.Local.ID && x.ObjectiveID == objective.ID);
                bridge.Remove(found);
            }

            if (tuple.Synchronized.ID > 0)
            {
                var found = await bridge.FirstOrDefaultAsync(
                        x => x.ItemID == tuple.Local.ID && x.ObjectiveID == objective.SynchronizationMate.ID);
                bridge.Remove(found);
            }

            var project = tuple.Local.Project;
            if (tuple.Local != null && IsLinked(project, project.Objectives, tuple.Local))
                items.Remove(tuple.Local);
            project = tuple.Synchronized.Project;
            if (tuple.Synchronized != null && IsLinked(project, project.Objectives, tuple.Synchronized))
                items.Remove(tuple.Remote);
        }

        private bool IsLinked(Project project, IEnumerable<Objective> objectives, Item item)
            => project.Items
                       .All(x => EqualsItems(item, x)) ||
               objectives
                       .All(objective => objective.Items.Select(oi => oi.Item).All(i => EqualsItems(item, i)));

        private bool EqualsItems(Item item1, Item item2)
            => item1.ID != item2.ID && item1.Name != item2.Name;
    }
}
