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
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization.Extensions;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Synchronization.Utils;
using MRS.DocumentManagement.Synchronization.Utils.Linkers;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal class ObjectiveStrategy : ASynchronizationStrategy<Objective, ObjectiveExternalDto>
    {
        private readonly ItemStrategy<ObjectiveItemLinker> itemStrategy;
        private readonly DynamicFieldStrategy<ObjectiveDynamicFieldLinker> dynamicFieldStrategy;

        public ObjectiveStrategy(
            DMContext context,
            IMapper mapper,
            ItemStrategy<ObjectiveItemLinker> itemStrategy,
            DynamicFieldStrategy<ObjectiveDynamicFieldLinker> dynamicFieldStrategy)
            : base(context, mapper)
        {
            this.itemStrategy = itemStrategy;
            this.dynamicFieldStrategy = dynamicFieldStrategy;
        }

        public override IReadOnlyCollection<Objective> Map(IReadOnlyCollection<ObjectiveExternalDto> externalDtos)
        {
            var objectives = base.Map(externalDtos);

            foreach (var objective in objectives)
            {
                var external = externalDtos.FirstOrDefault(x => x.ExternalID == objective.ExternalID);
                if (!string.IsNullOrEmpty(external.ParentObjectiveExternalID))
                {
                    objective.ParentObjective =
                        objectives.FirstOrDefault(x => x.ExternalID == external.ParentObjectiveExternalID);
                }
            }

            return objectives;
        }

        protected override DbSet<Objective> GetDBSet(DMContext context)
            => context.Objectives;

        protected override ISynchronizer<ObjectiveExternalDto> GetSynchronizer(IConnectionContext context)
            => context.ObjectivesSynchronizer;

        protected override Expression<Func<Objective, bool>> GetDefaultFilter(SynchronizingData data)
            => data.ObjectivesFilter;

        protected override IIncludableQueryable<Objective, Objective> Include(IQueryable<Objective> set)
            => base.Include(
                set.Include(x => x.Items)
                   .Include(x => x.ParentObjective)
                        .ThenInclude(x => x.SynchronizationMate)
                   .Include(x => x.Author)
                   .Include(x => x.BimElements)
                        .ThenInclude(x => x.BimElement));

        protected override IEnumerable<Objective> Order(IEnumerable<Objective> objectives)
            => objectives.OrderByParent(x => x.ParentObjective);

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            try
            {
                tuple.Merge();
                CreateObjectiveParentLink(data, tuple);
                var project = await context.Projects.Include(x => x.SynchronizationMate)
                   .FirstOrDefaultAsync(x => x.ID == tuple.Local.ProjectID);
                tuple.Synchronized.ProjectID = project?.SynchronizationMateID ?? 0;
                tuple.Remote.ProjectID = tuple.Synchronized.ProjectID;

                MergeBimElements(tuple);
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, token);
                if (resultAfterChildrenSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing children in Add Objective To Remote");

                var result = await base.AddToRemote(tuple, data, connectionContext, parent, token);
                UpdateChildrenAfterSynchronization(tuple);
                return result;
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
            object parent,
            CancellationToken token)
        {
            try
            {
                tuple.Merge();
                CreateObjectiveParentLink(data, tuple);
                tuple.Synchronized.ProjectID = tuple.Remote.ProjectID;
                var id = tuple.Synchronized.ProjectID;
                tuple.Local.Project = await context.Projects
                   .FirstOrDefaultAsync(x => x.SynchronizationMateID == id);

                MergeBimElements(tuple);
                var resultAfterBase = await base.AddToLocal(tuple, data, connectionContext, parent, token);
                if (resultAfterBase != null)
                    throw resultAfterBase.Exception;

                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, token);
                if (resultAfterChildrenSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing children in Add Objective To Local");

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
            object parent,
            CancellationToken token)
        {
            try
            {
                MergeBimElements(tuple);
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, token);
                if (resultAfterChildrenSync.Count > 0)
                    throw new Exception($"Exception created while Synchronizing children in Merge Objective");

                var result = await base.Merge(tuple, data, connectionContext, parent, token);
                UpdateChildrenAfterSynchronization(tuple);
                return result;
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
            object parent,
            CancellationToken token)
        {
            try
            {
                return await base.RemoveFromLocal(tuple, data, connectionContext, parent, token);
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

        protected override async Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            try
            {
                return await base.RemoveFromRemote(tuple, data, connectionContext, parent, token);
            }
            catch (Exception e)
            {
                return new SynchronizingResult
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
            IConnectionContext connectionContext,
            CancellationToken token)
        {
            var id1 = tuple.Local?.ID ?? 0; // 1
            var id2 = tuple.Synchronized?.ID ?? 0; // 0
            var itemsResult = await itemStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.Items?.Select(x => x.Item).ToList() ?? new List<Item>(), // new list
                token,
                item
                    => item.Objectives.Any(x => x.ObjectiveID == id1 || x.ObjectiveID == id2) ||
                    (item.SynchronizationMate != null &&
                        item.SynchronizationMate.Objectives.Any(x => x.ObjectiveID == id1 || x.ObjectiveID == id2)),
                null,
                tuple);
            var fieldResult = await dynamicFieldStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.DynamicFields?.ToList() ?? new List<DynamicField>(),
                token,
                field => field.ObjectiveID == id1 || field.ObjectiveID == id2 ||
                    (field.SynchronizationMate != null &&
                        (field.SynchronizationMate.ObjectiveID == id1 || field.SynchronizationMate.ObjectiveID == id2)),
                null,
                tuple);

            return itemsResult.Concat(fieldResult).ToList();
        }

        private void UpdateChildrenAfterSynchronization(SynchronizingTuple<Objective> tuple)
        {
            ItemStrategy<ObjectiveItemLinker>.UpdateExternalIDs(
                (tuple.Local.Items ?? ArraySegment<ObjectiveItem>.Empty)
               .Concat(tuple.Synchronized.Items ?? ArraySegment<ObjectiveItem>.Empty)
               .Select(x => x.Item),
                (tuple.Remote.Items ?? ArraySegment<ObjectiveItem>.Empty).Select(x => x.Item).ToArray());
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

        private void CreateObjectiveParentLink(SynchronizingData data, SynchronizingTuple<Objective> tuple)
        {
            if (tuple.Local.ParentObjective != null)
            {
                tuple.Synchronized.ParentObjective =
                    tuple.Remote.ParentObjective = tuple.Local.ParentObjective.SynchronizationMate;
            }
            else if (tuple.Remote.ParentObjective != null)
            {
                var allObjectives = context.Objectives.Local.Concat(context.Objectives).ToList();

                // ReSharper disable once PossibleMultipleEnumeration
                tuple.Synchronized.ParentObjective = allObjectives
                   .First(
                        x => string.Equals(x.ExternalID, tuple.Remote.ParentObjective.ExternalID) && x.IsSynchronized);

                // ReSharper disable once PossibleMultipleEnumeration
                tuple.Local.ParentObjective = allObjectives
                   .First(x => string.Equals(x.ExternalID, tuple.Remote.ParentObjective.ExternalID) && !x.IsSynchronized);
            }
        }
    }
}
