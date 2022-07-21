using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class ObjectiveStrategy : ISynchronizationStrategy<Objective>
    {
        private readonly DMContext context;
        private readonly IExternalIdUpdater<DynamicField> dynamicFieldIdUpdater;
        private readonly IExternalIdUpdater<Item> itemIdUpdater;
        private readonly ILogger<ObjectiveStrategy> logger;
        private readonly IMerger<Objective> merger;
        private readonly StrategyHelper strategyHelper;

        public ObjectiveStrategy(
            StrategyHelper strategyHelper,
            DMContext context,
            IMerger<Objective> merger,
            IExternalIdUpdater<DynamicField> dynamicFieldIdUpdater,
            IExternalIdUpdater<Item> itemIdUpdater,
            ILogger<ObjectiveStrategy> logger)
        {
            this.strategyHelper = strategyHelper;
            this.context = context;
            this.merger = merger;
            this.dynamicFieldIdUpdater = dynamicFieldIdUpdater;
            this.itemIdUpdater = itemIdUpdater;
            this.logger = logger;
            logger.LogTrace("ObjectiveStrategy created");
        }

        public DbSet<Objective> GetDBSet(DMContext source)
            => source.Objectives;

        public async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);

                UpdateChildrenBeforeSynchronization(tuple, data);
                CreateObjectiveParentLink(tuple);
                tuple.Synchronized.ProjectID = tuple.Remote.ProjectID;
                var id = tuple.Synchronized.ProjectID;
                tuple.Local.ProjectID = await context.Projects
                   .AsNoTracking()
                   .Where(x => x.SynchronizationMateID == id)
                   .Select(x => x.ID)
                   .FirstOrDefaultAsync(CancellationToken.None)
                   .ConfigureAwait(false);

                var resultAfterBase = await strategyHelper.AddToLocal(tuple, token).ConfigureAwait(false);
                if (resultAfterBase != null)
                    throw resultAfterBase.Exception;

                await merger.Merge(tuple).ConfigureAwait(false);
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

        public async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);

                UpdateChildrenBeforeSynchronization(tuple, data);
                CreateObjectiveParentLink(tuple);
                var projectID = tuple.Local.ProjectID;
                var projectMateId = await context.Projects
                   .AsNoTracking()
                   .Where(x => x.ID == projectID)
                   .Where(x => x.SynchronizationMateID != null)
                   .Select(x => x.SynchronizationMateID)
                   .FirstOrDefaultAsync(CancellationToken.None)
                   .ConfigureAwait(false);
                tuple.Remote.ProjectID = tuple.Synchronized.ProjectID = projectMateId ?? 0;

                var result = await strategyHelper
                   .AddToRemote(data.ConnectionContext.ObjectivesSynchronizer, tuple, token)
                   .ConfigureAwait(false);
                await UpdateChildrenAfterSynchronization(tuple, data).ConfigureAwait(false);
                await merger.Merge(tuple).ConfigureAwait(false);
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

        public Expression<Func<Objective, bool>> GetFilter(SynchronizingData data)
            => data.ObjectivesFilter;

        public async Task<SynchronizingResult> Merge(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);
                UpdateChildrenBeforeSynchronization(tuple, data);
                var result = await strategyHelper.Merge(tuple, data.ConnectionContext.ObjectivesSynchronizer, token)
                   .ConfigureAwait(false);
                await UpdateChildrenAfterSynchronization(tuple, data).ConfigureAwait(false);
                await merger.Merge(tuple).ConfigureAwait(false);
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

        public IEnumerable<Objective> Order(IEnumerable<Objective> enumeration)
            => enumeration.OrderByParent(x => x.ParentObjective);

        public async Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                return await strategyHelper.RemoveFromLocal(tuple, token).ConfigureAwait(false);
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

        public async Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                return await strategyHelper
                   .RemoveFromRemote(data.ConnectionContext.ObjectivesSynchronizer, tuple, token)
                   .ConfigureAwait(false);
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

        private static void AddConnectionInfoToDynamicFields(SynchronizingTuple<Objective> tuple, SynchronizingData data)
        {
            if (tuple.Remote == null)
                return;

            var remoteObjective = tuple.Remote;

            foreach (var df in remoteObjective.DynamicFields)
                AddConnectionInfoTo(df);

            void AddConnectionInfoTo(DynamicField df)
            {
                df.ConnectionInfoID ??= data.User.ConnectionInfoID;

                if (df.ChildrenDynamicFields == null)
                    return;

                foreach (var child in df.ChildrenDynamicFields)
                    AddConnectionInfoTo(child);
            }
        }

        private static void AddProjectToRemote(SynchronizingTuple<Objective> tuple)
        {
            tuple.Remote.Project ??= tuple.Synchronized.Project;
            if (tuple.Remote.ProjectID == 0)
                tuple.Remote.ProjectID = tuple.Synchronized.ProjectID;
        }

        private static void AddProjectToRemoteItems(SynchronizingTuple<Objective> tuple)
        {
            if (tuple.Remote.Items == null)
                return;

            foreach (var link in tuple.Remote.Items)
            {
                var item = link.Item;
                item.ProjectID = tuple.AsEnumerable().Select(x => x.ProjectID).First(x => x != 0);
            }
        }

        private void CreateObjectiveParentLink(SynchronizingTuple<Objective> tuple)
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

        private void StartTrackAuthor(SynchronizingTuple<Objective> synchronizingTuple)
        {
            var author = synchronizingTuple.Local.Author;
            if (context.Entry(author).State == EntityState.Detached)
                context.Attach(author);
        }

        private Task UpdateChildrenAfterSynchronization(SynchronizingTuple<Objective> tuple, SynchronizingData data)
        {
            logger.LogTrace("UpdateChildrenAfterSynchronization started with {@Tuple}", tuple);
            itemIdUpdater.UpdateExternalIds(
                (tuple.Local.Items ?? ArraySegment<ObjectiveItem>.Empty)
               .Concat(tuple.Synchronized.Items ?? ArraySegment<ObjectiveItem>.Empty)
               .Select(x => x.Item),
                (tuple.Remote.Items ?? ArraySegment<ObjectiveItem>.Empty).Select(x => x.Item).ToArray());
            logger.LogTrace("External ids of items updated");
            dynamicFieldIdUpdater.UpdateExternalIds(
                (tuple.Local.DynamicFields ?? ArraySegment<DynamicField>.Empty)
               .Concat(tuple.Synchronized.DynamicFields ?? ArraySegment<DynamicField>.Empty),
                tuple.Remote.DynamicFields ?? ArraySegment<DynamicField>.Empty);
            logger.LogTrace("External ids of dynamic fields updated");

            AddConnectionInfoToDynamicFields(tuple, data);
            AddProjectToRemote(tuple);
            return Task.CompletedTask;
        }

        private void UpdateChildrenBeforeSynchronization(SynchronizingTuple<Objective> tuple, SynchronizingData data)
        {
            StartTrackAuthor(tuple);
            AddConnectionInfoToDynamicFields(tuple, data);
            AddProjectToRemoteItems(tuple);
        }
    }
}
