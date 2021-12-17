using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils;
using Brio.Docs.Synchronization.Utils.Linkers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class ObjectiveStrategy : ASynchronizationStrategy<Objective, ObjectiveExternalDto>
    {
        private readonly ItemStrategy<ObjectiveItemLinker> itemStrategy;
        private readonly DynamicFieldStrategy<ObjectiveDynamicFieldLinker> dynamicFieldStrategy;
        private readonly ILogger<ObjectiveStrategy> logger;

        public ObjectiveStrategy(
            DMContext context,
            IMapper mapper,
            ItemStrategy<ObjectiveItemLinker> itemStrategy,
            DynamicFieldStrategy<ObjectiveDynamicFieldLinker> dynamicFieldStrategy,
            ILogger<ObjectiveStrategy> logger)
            : base(context, mapper, logger)
        {
            this.itemStrategy = itemStrategy;
            this.dynamicFieldStrategy = dynamicFieldStrategy;
            this.logger = logger;
            logger.LogTrace("ObjectiveStrategy created");
        }

        public override IReadOnlyCollection<Objective> Map(IReadOnlyCollection<ObjectiveExternalDto> externalDtos)
        {
            logger.LogTrace("Map started");
            var objectives = base.Map(externalDtos);

            foreach (var objective in objectives)
            {
                var external = externalDtos.FirstOrDefault(x => x.ExternalID == objective.ExternalID);
                if (!string.IsNullOrEmpty(external?.ParentObjectiveExternalID))
                {
                    objective.ParentObjective =
                        objectives.FirstOrDefault(x => x.ExternalID == external.ParentObjectiveExternalID);
                }
            }

            return objectives;
        }

        protected override DbSet<Objective> GetDBSet(DMContext source)
            => source.Objectives;

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
                        .ThenInclude(x => x.BimElement)
                   .Include(x => x.Location)
                        .ThenInclude(x => x.Item));

        protected override IEnumerable<Objective> Order(IEnumerable<Objective> objectives)
            => objectives.OrderByParent(x => x.ParentObjective);

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                logger.LogBeforeMerge(tuple);
                tuple.Merge();
                logger.LogAfterMerge(tuple);
                CreateObjectiveParentLink(data, tuple);
                logger.LogTrace("Created parent link");
                var projectID = tuple.Local.ProjectID;
                var project = await context.Projects.Include(x => x.SynchronizationMate)
                   .FirstOrDefaultAsync(x => x.ID == projectID, CancellationToken.None);
                tuple.Synchronized.ProjectID = project?.SynchronizationMateID ?? 0;
                tuple.Remote.ProjectID = tuple.Synchronized.ProjectID;
                logger.LogTrace("Created links for project ids");

                MergeBimElements(tuple);
                logger.LogTrace("Bim elements synchronized");
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, token);
                logger.LogTrace("Children synchronized");
                if (resultAfterChildrenSync.Count > 0)
                    throw new Exception("Exception created while Synchronizing children in Add Objective To Remote");

                var result = await base.AddToRemote(tuple, data, connectionContext, parent, token);
                await UpdateChildrenAfterSynchronization(tuple);
                logger.LogTrace("Children updated");
                return result;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToRemote, e, tuple);
                return new SynchronizingResult
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                logger.LogBeforeMerge(tuple);
                tuple.Merge();
                logger.LogAfterMerge(tuple);
                CreateObjectiveParentLink(data, tuple);
                logger.LogTrace("Created parent link");
                tuple.Synchronized.ProjectID = tuple.Remote.ProjectID;
                var id = tuple.Synchronized.ProjectID;
                tuple.Local.Project = await context.Projects
                   .FirstOrDefaultAsync(x => x.SynchronizationMateID == id);
                logger.LogTrace("Created links for project ids");

                MergeBimElements(tuple);
                logger.LogTrace("Bim elements synchronized");
                var resultAfterBase = await base.AddToLocal(tuple, data, connectionContext, parent, token);
                if (resultAfterBase != null)
                    throw resultAfterBase.Exception;

                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, token);
                logger.LogTrace("Children synchronized");
                if (resultAfterChildrenSync.Count > 0)
                    throw new Exception("Exception created while Synchronizing children in Add Objective To Local");

                return null;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToLocal, e, tuple);
                return new SynchronizingResult
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                logger.LogBeforeMerge(tuple);
                tuple.Merge();
                logger.LogAfterMerge(tuple);
                MergeBimElements(tuple);
                logger.LogTrace("Bim elements synchronized");
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, token);
                logger.LogTrace("Children synchronized");
                if (resultAfterChildrenSync.Count > 0)
                    throw new Exception("Exception created while Synchronizing children in Merge Objective");

                var result = await base.Merge(tuple, data, connectionContext, parent, token);
                await UpdateChildrenAfterSynchronization(tuple);
                await SaveDb(data);

                resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, token);
                logger.LogTrace("Children synchronized");
                if (resultAfterChildrenSync.Count > 0)
                    throw new Exception("Exception created while Synchronizing children in Merge Objective");

                await UpdateChildrenAfterSynchronization(tuple);
                logger.LogTrace("Children updated");

                return result;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.Merge, e, tuple);
                return new SynchronizingResult
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                return await base.RemoveFromLocal(tuple, data, connectionContext, parent, token);
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromLocal, e, tuple);
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
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                return await base.RemoveFromRemote(tuple, data, connectionContext, parent, token);
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromRemote, e, tuple);
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
            logger.LogStartAction(tuple, data, data);
            var id1 = tuple.Local?.ID ?? 0;
            var id2 = tuple.Synchronized?.ID ?? 0;
            logger.LogDebug("Local id = {@Local}, Synchronized id = {@Synchronized}", id1, id2);
            var itemsResult = await itemStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.Items?.Select(x => x.Item).ToList() ?? new List<Item>(), // new list
                token,
                item => item.Objectives.Any(x => x.ObjectiveID == id1 || x.ObjectiveID == id2),
                null,
                tuple);
            logger.LogTrace("Items of objective synchronized");
            var fieldResult = await dynamicFieldStrategy.Synchronize(
                data,
                connectionContext,
                tuple.Remote?.DynamicFields?.ToList() ?? new List<DynamicField>(),
                token,
                field => field.ObjectiveID == id1 || field.ObjectiveID == id2,
                null,
                tuple);
            logger.LogTrace("Dynamic fields synchronized");

            await LinkLocationItem(tuple);
            logger.LogTrace("Location item linked");

            return itemsResult.Concat(fieldResult).ToList();
        }

        private async Task UpdateChildrenAfterSynchronization(SynchronizingTuple<Objective> tuple)
        {
            logger.LogTrace("UpdateChildrenAfterSynchronization started with {@Tuple}", tuple);
            ItemStrategy<ObjectiveItemLinker>.UpdateExternalIDs(
                (tuple.Local.Items ?? ArraySegment<ObjectiveItem>.Empty)
               .Concat(tuple.Synchronized.Items ?? ArraySegment<ObjectiveItem>.Empty)
               .Select(x => x.Item),
                (tuple.Remote.Items ?? ArraySegment<ObjectiveItem>.Empty).Select(x => x.Item).ToArray());
            logger.LogTrace("External ids of items updated");
            DynamicFieldStrategy<ObjectiveDynamicFieldLinker>.UpdateExternalIDs(
                (tuple.Local.DynamicFields ?? ArraySegment<DynamicField>.Empty)
               .Concat(tuple.Synchronized.DynamicFields ?? ArraySegment<DynamicField>.Empty),
                tuple.Remote.DynamicFields ?? ArraySegment<DynamicField>.Empty);
            logger.LogTrace("External ids of dynamic fields updated");

            await LinkLocationItem(tuple);
            logger.LogTrace("Location item linked");
        }

        private void MergeBimElements(SynchronizingTuple<Objective> tuple)
        {
            logger.LogTrace("MergeBimElements started with {@Tuple}", tuple);
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
            logger.LogTrace("CreateObjectiveParentLink started with {@Tuple}", tuple);
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

        // TODO: improve this
        private async Task LinkLocationItem(SynchronizingTuple<Objective> tuple)
        {
            logger.LogTrace("LinkLocationItem started with {@Tuple}", tuple);
            if (tuple.Local!.Location != null &&
                (tuple.Local.Location.Item == null || tuple.Synchronized.Location.Item == null || tuple.Remote.Location.Item == null))
            {
                var itemTuple = new SynchronizingTuple<Item>(
                    local: tuple.Local.Location!.Item,
                    synchronized: tuple.Synchronized!.Location.Item,
                    remote: tuple.Remote!.Location.Item);

                await itemStrategy.FindAndAttachExists(
                    itemTuple,
                    null,
                    tuple);

                if (itemTuple.Synchronized != null && itemTuple.Remote == null)
                {
                    logger.LogDebug("Creating remote");

                    itemTuple.Remote = new Item
                    {
                        ExternalID = itemTuple.Synchronized.ExternalID,
                        ItemType = itemTuple.Synchronized.ItemType,
                        RelativePath = itemTuple.Synchronized.RelativePath,
                        ProjectID = itemTuple.Synchronized.ProjectID,
                    };
                    logger.LogDebug("Created item: {@Object}", tuple.Local);
                    itemTuple.RemoteChanged = true;
                }

                tuple.Local.Location.Item = itemTuple.Local;
                tuple.Synchronized.Location.Item = itemTuple.Synchronized ?? itemTuple.Local?.SynchronizationMate;
                tuple.Remote.Location.Item = itemTuple.Remote;
            }
        }
    }
}
