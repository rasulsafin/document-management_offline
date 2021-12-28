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
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils.Linkers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class ObjectiveStrategy : ASynchronizationStrategy<Objective, ObjectiveExternalDto>
    {
        private readonly IMerger<Objective> merger;
        private readonly ILogger<ObjectiveStrategy> logger;

        public ObjectiveStrategy(
            DMContext context,
            IMapper mapper,
            IMerger<Objective> merger,
            ILogger<ObjectiveStrategy> logger)
            : base(context, mapper, logger)
        {
            this.merger = merger;
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
                await merger.Merge(tuple).ConfigureAwait(false);

                CreateObjectiveParentLink(data, tuple);
                var projectID = tuple.Local.ProjectID;
                var projectMateId = await context.Projects
                   .AsNoTracking()
                   .Where(x => x.ID == projectID)
                   .Where(x => x.SynchronizationMateID != null)
                   .Select(x => x.SynchronizationMateID)
                   .FirstOrDefaultAsync(CancellationToken.None)
                   .ConfigureAwait(false);
                tuple.Remote.ProjectID = tuple.Synchronized.ProjectID = projectMateId ?? 0;

                var result = await base.AddToRemote(tuple, data, connectionContext, parent, token).ConfigureAwait(false);
                await UpdateChildrenAfterSynchronization(tuple).ConfigureAwait(false);
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
                await merger.Merge(tuple).ConfigureAwait(false);

                CreateObjectiveParentLink(data, tuple);
                tuple.Synchronized.ProjectID = tuple.Remote.ProjectID;
                var id = tuple.Synchronized.ProjectID;
                tuple.Local.ProjectID = await context.Projects
                   .AsNoTracking()
                   .Where(x => x.SynchronizationMateID == id)
                   .Select(x => x.ID)
                   .FirstOrDefaultAsync(CancellationToken.None)
                   .ConfigureAwait(false);

                var resultAfterBase = await base.AddToLocal(tuple, data, connectionContext, parent, token).ConfigureAwait(false);
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
                await merger.Merge(tuple).ConfigureAwait(false);
                var result = await base.Merge(tuple, data, connectionContext, parent, token).ConfigureAwait(false);
                await UpdateChildrenAfterSynchronization(tuple).ConfigureAwait(false);
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

            logger.LogTrace("Location item linked");
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
    }
}
