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
using MRS.DocumentManagement.Synchronization.Extensions;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Utils;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal class ObjectiveStrategy : ASynchronizationStrategy<Objective, ObjectiveExternalDto>
    {
        private readonly ItemStrategy itemStrategy;
        private readonly DynamicFieldStrategy dynamicFieldStrategy;

        public ObjectiveStrategy(IMapper mapper)
            : base(mapper)
        {
            itemStrategy = new ItemStrategy(mapper, LinkItem, UnlinkItem);
            dynamicFieldStrategy = new DynamicFieldStrategy(mapper, LinkDynamicField, UpdateDynamicField, UnlinkDynamicField);
        }

        protected override DbSet<Objective> GetDBSet(DMContext context)
            => context.Objectives;

        protected override ISynchronizer<ObjectiveExternalDto> GetSynchronizer(IConnectionContext context)
            => context.ObjectivesSynchronizer;

        protected override Expression<Func<Objective, bool>> GetDefaultFilter(SynchronizingData data)
            => data.ObjectivesFilter;

        protected override IIncludableQueryable<Objective, Objective> Include(IQueryable<Objective> set)
            => base.Include(set.Include(x => x.Items));

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                tuple.Merge();
                tuple.Synchronized.Project = tuple.Local.Project.SynchronizationMate;
                tuple.Remote.Project = tuple.Synchronized.Project;

                MergeBimElements(tuple);
                var resultAfterItemSync = await SynchronizeChildren(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Add Objective To Remote");

                return await base.AddToRemote(tuple, data, connectionContext, parent);
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

        protected override async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                tuple.Merge();
                tuple.Synchronized.Project = tuple.Remote.Project;
                tuple.Local.Project = data.Context.Projects
                   .FirstOrDefault(x => x.SynchronizationMateID == tuple.Synchronized.Project.ID);

                MergeBimElements(tuple);
                var resultAfterBase = await base.AddToLocal(tuple, data, connectionContext, parent);
                if (resultAfterBase != null)
                    throw resultAfterBase.Exception;

                var resultAfterItemSync = await SynchronizeChildren(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Add Objective To Local");

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

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                MergeBimElements(tuple);
                var resultAfterItemSync = await SynchronizeChildren(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Merge Objective");

                return await base.Merge(tuple, data, connectionContext, parent);
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

        protected override async Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterItemSync = await SynchronizeChildren(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Remove Objective From Local");

                return await base.RemoveFromLocal(tuple, data, connectionContext, parent);
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

        protected override async Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                var resultAfterItemSync = await SynchronizeChildren(tuple, data, connectionContext);
                if (resultAfterItemSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing Items in Remove Objective From Remote");

                return await base.RemoveFromRemote(tuple, data, connectionContext, parent);
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

        private async Task<List<SynchronizingResult>> SynchronizeChildren(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext)
        {
            var id1 = tuple.Local?.ID ?? 0;
            var id2 = tuple.Synchronized?.ID ?? 0;
            await itemStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.Items?.Select(x => x.Item).ToList() ?? new List<Item>(),
                item
                    => item.Objectives.Any(x => x.ObjectiveID == id1 || x.ObjectiveID == id2) ||
                    (item.SynchronizationMate != null &&
                        item.SynchronizationMate.Objectives.Any(x => x.ObjectiveID == id1 || x.ObjectiveID == id2)),
                tuple);
            await dynamicFieldStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.DynamicFields?.ToList() ?? new List<DynamicField>(),
                field => field.ObjectiveID == id1 || field.ObjectiveID == id2 ||
                    (field.SynchronizationMate != null &&
                        (field.SynchronizationMate.ObjectiveID == id1 || field.SynchronizationMate.ObjectiveID == id2)),
                tuple);
        }

        private Task LinkItem(DMContext context, Item item, object parent, EntityType entityType)
        {
            var objective = LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);
            objective.Items ??= new List<ObjectiveItem>();
            objective.Items.Add(new ObjectiveItem
            {
                Item = item,
                ObjectiveID = objective.ID,
            });
            return Task.CompletedTask;
        }

        private Task UnlinkItem(DMContext context, Item item, object parent, EntityType entityType)
        {
            var objective =  LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);

            if (entityType == EntityType.Remote)
            {
                objective.Items.Remove(objective.Items.First(x => Equals(x.Item, item)));
                return Task.CompletedTask;
            }

            item.Objectives.Remove(item.Objectives.First(x => Equals(x.Objective, objective)));
            if (item.Project == null && item.Objectives?.Count == 0)
                context.Items.Remove(item);
            return Task.CompletedTask;
        }

        private Task UnlinkDynamicField(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            var objective =  LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);
            field.Objective = null;

            if (entityType == EntityType.Remote)
                objective.DynamicFields.Remove(field);
            else if (field.ParentFieldID == null)
                context.DynamicFields.Remove(field);
            else
                context.DynamicFields.Update(field);
            return Task.CompletedTask;
        }

        private Task UpdateDynamicField(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);
            if (entityType != EntityType.Remote)
                context.DynamicFields.Update(field);
            return Task.CompletedTask;
        }

        private Task LinkDynamicField(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            var objective = LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);
            objective.DynamicFields ??= new List<DynamicField>();
            objective.DynamicFields.Add(field);
            return Task.CompletedTask;
        }

        private void MergeBimElements(SynchronizingTuple<Objective> tuple)
        {
            tuple.Local.BimElements ??= new List<BimElementObjective>();
            tuple.Synchronized.BimElements ??= new List<BimElementObjective>();
            tuple.Remote.BimElements ??= new List<BimElementObjective>();

            CollectionUtils.Merge(
                tuple,
                tuple.Local.BimElements,
                tuple.Synchronized.BimElements,
                tuple.Remote.BimElements,
                (a, b) => string.Equals(a.ParentName, b.ParentName, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(a.GlobalID, b.GlobalID),
                (element, objective) => new BimElementObjective
                {
                    BimElement = element,
                    Objective = objective,
                },
                x => x.BimElement);
        }
    }
}
