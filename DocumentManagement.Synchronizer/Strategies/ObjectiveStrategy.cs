using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Strategies
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

        protected override ISynchronizer<ObjectiveExternalDto> GetSynchronizer(IConnectionContext context)
            => context.ObjectivesSynchronizer;

        protected override Expression<Func<Objective, bool>> GetDefaultFilter(SynchronizingData data)
            => data.ObjectivesFilter;

        protected override IIncludableQueryable<Objective, Objective> Include(IQueryable<Objective> set)
            => base.Include(set.Include(x => x.Items));

        protected override async Task AddToRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.AddToRemote(tuple, data, connectionContext, parent);
        }

        protected override async Task AddToLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await base.AddToLocal(tuple, data, connectionContext, parent);
            await SynchronizeItems(tuple, data, connectionContext);
        }

        protected override async Task Merge(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.Merge(tuple, data, connectionContext, parent);
        }

        protected override async Task RemoveFromLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.RemoveFromLocal(tuple, data, connectionContext, parent);
        }

        protected override async Task RemoveFromRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await SynchronizeItems(tuple, data, connectionContext);
            await base.RemoveFromRemote(tuple, data, connectionContext, parent);
        }

        private async Task SynchronizeItems(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext)
        {
            var id1 = tuple.Local.ID;
            var id2 = tuple.Synchronized.ID;
            await itemStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote.Items.Select(x => x.Item).ToList(),
                item
                    => item.Objectives.Any(x => x.ObjectiveID == id1 || x.ObjectiveID == id2) ||
                    (item.SynchronizationMate != null &&
                        item.SynchronizationMate.Objectives.Any(x => x.ObjectiveID == id1 || x.ObjectiveID == id2)),
                tuple);
        }

        private Task Link(Item item, object parent, EntityType entityType)
        {
            var tuple = (SynchronizingTuple<Objective>)parent;
            var objective = item.IsSynchronized ? tuple.Synchronized : tuple.Local;
            if (objective == null)
            {
                throw new ArgumentException(
                    $"Parent doesn't contain {(item.IsSynchronized ? "synchronized" : "unsynchronized")} objective");
            }

            objective.Items ??= new List<ObjectiveItem>();
            objective.Items.Add(new ObjectiveItem
            {
                Item = item,
                ObjectiveID = objective.ID,
            });

            return Task.CompletedTask;
        }

        private Task Unlink(Item item, object parent, EntityType entityType)
        {

            return Task.CompletedTask;
        }
    }
}
