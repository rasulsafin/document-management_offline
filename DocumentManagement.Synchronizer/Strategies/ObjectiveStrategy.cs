using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronizer.Models;

namespace MRS.DocumentManagement.Synchronizer.Strategies
{
    internal class ObjectiveStrategy : ASynchronizationStrategy<Objective, ObjectiveExternalDto>
    {
        private readonly ItemStrategy itemStrategy;

        public ObjectiveStrategy(IMapper mapper)
            : base(mapper)
        {
            this.itemStrategy = new ItemStrategy(mapper, Link, Unlink);
        }

        protected override DbSet<Objective> GetDBSet(DMContext context)
            => context.Objectives;

        protected override ISynchronizer<ObjectiveExternalDto> GetSynchronizer(AConnectionContext context)
            => context.ObjectivesSynchronizer;

        protected override bool DefaultFilter(SynchronizingData data, Objective objective)
            => data.ObjectivesFilter(objective);

        protected override IIncludableQueryable<Objective, Objective> Include(IQueryable<Objective> set)
            => base.Include(set.Include(x => x.Items));

        protected override async Task AddToRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.AddToRemote(tuple, data, connectionContext, parent);
        }

        protected override async Task AddToLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
        {
            await base.AddToLocal(tuple, data, connectionContext, parent);
            await SynchronizeItems(tuple, data, connectionContext);
        }

        protected override async Task Merge(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.Merge(tuple, data, connectionContext, parent);
        }

        protected override async Task RemoveFromLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.RemoveFromLocal(tuple, data, connectionContext, parent);
        }

        protected override async Task RemoveFromRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.RemoveFromRemote(tuple, data, connectionContext, parent);
        }

        private async Task SynchronizeItems(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext)
        {
            var items = await data.Context.ObjectiveItems
               .Where(x => x.ObjectiveID == tuple.Local.ID || x.ObjectiveID == tuple.Synchronized.ID)
               .ToListAsync();
            await itemStrategy.Synchronize(data, connectionContext, item => items.Any(x => x.ItemID == item.ID), tuple);
        }

        private Task Link(Item item, object parent, SynchronizingData data)
        {
            var tuple = (SynchronizingTuple<Objective>)parent;
            var objective = item.IsSynchronized ? tuple.Synchronized : tuple.Local;
            if (objective == null)
                throw new ArgumentException();

            if (item.Objectives.All(x => x.ObjectiveID != objective.ID))
            {
                data.Context.ObjectiveItems.Add(new ObjectiveItem
                {
                    ItemID = item.ID,
                    ObjectiveID = objective.ID,
                });
            }

            return Task.CompletedTask;
        }

        private Task Unlink(Item item, object parent, SynchronizingData data)
        {
            var parentTuple = (SynchronizingTuple<Objective>)parent;
            var objective = item.IsSynchronized ? parentTuple.Synchronized : parentTuple.Local;
            var link = item.Objectives.FirstOrDefault(x => x.ObjectiveID == objective.ID);
            item.Objectives.Remove(link);
            if (item.ProjectID != null || item.Objectives.Count > 0)
                data.Context.Items.Update(item);
            else
                data.Context.Items.Remove(item);
            return Task.CompletedTask;
        }
    }
}
