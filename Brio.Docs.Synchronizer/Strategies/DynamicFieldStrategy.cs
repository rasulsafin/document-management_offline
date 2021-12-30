using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class DynamicFieldStrategy<TLinker> : ALinkingStrategy<DynamicField, DynamicFieldExternalDto>
        where TLinker : ILinker<DynamicField>
    {
        private readonly Func<DynamicFieldStrategy<DynamicFieldDynamicFieldLinker>> getSubstrategy;
        private readonly ILogger<DynamicFieldStrategy<TLinker>> logger;

        public DynamicFieldStrategy(
            DMContext context,
            IMapper mapper,
            TLinker linker,
            Func<DynamicFieldStrategy<DynamicFieldDynamicFieldLinker>> getSubstrategy,
            ILogger<DynamicFieldStrategy<TLinker>> logger)
            : base(context, mapper, linker, logger)
        {
            this.getSubstrategy = getSubstrategy;
            this.logger = logger;
            logger.LogTrace("DynamicFieldStrategy created");
        }

        public static void UpdateExternalIDs(IEnumerable<DynamicField> local, IEnumerable<DynamicField> remote)
        {
            var remotes = remote.ToList();
            var dfComparer = new DynamicFieldComparer();

            foreach (var dynamicField in local)
                UpdateExternalIDs(remotes, dynamicField, dfComparer);
        }

        protected override IIncludableQueryable<DynamicField, DynamicField> Include(IQueryable<DynamicField> set)
            => base.Include(set.Include(x => x.ParentField));

        protected override IEnumerable<DynamicField> Order(IEnumerable<DynamicField> list)
            => list.OrderByParent(x => x.ParentField);

        protected override async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                tuple.Remote.ConnectionInfoID = data.User.ConnectionInfoID;
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent, token);

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Add Dynamic Field To Local");

                return await base.AddToLocal(tuple, data, connectionContext, parent, token);
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

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<DynamicField> tuple,
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
                tuple.Remote.ExternalID = tuple.Local.ExternalID;

                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent, token);
                logger.LogTrace("Children synchronized");

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Add Dynamic Field To Remote");

                return await base.AddToRemote(tuple, data, connectionContext, parent, token);
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

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                tuple.Remote.ConnectionInfoID = data.User.ConnectionInfoID;
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent, token);
                logger.LogTrace("Children synchronized");

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Merge Dynamic Field");

                return await base.Merge(tuple, data, connectionContext, parent, token);
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
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent, token);
                logger.LogTrace("Children synchronized");

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Remove Dynamic Field From Local");

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
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                var resultAfterChildrenSync = await SynchronizeChildren(tuple, data, connectionContext, parent, token);
                logger.LogTrace("Children synchronized");

                if ((resultAfterChildrenSync?.Count ?? 0) > 0)
                    throw new Exception("Exception created while Synchronizing children in Remove Dynamic Field From Remote");

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

        protected override DbSet<DynamicField> GetDBSet(DMContext context)
            => context.DynamicFields;

        private static void UpdateExternalIDs(IEnumerable<DynamicField> remote, DynamicField dynamicField, IEqualityComparer<DynamicField> comparer)
        {
            var found = remote.FirstOrDefault(x => comparer.Equals(dynamicField, x));
            if (found == null)
                return;

            dynamicField.ExternalID = found.ExternalID;
            foreach (var child in dynamicField.ChildrenDynamicFields ?? Enumerable.Empty<DynamicField>())
                UpdateExternalIDs(found.ChildrenDynamicFields.ToList(), child, comparer);
        }

        private async Task<List<SynchronizingResult>> SynchronizeChildren(
            SynchronizingTuple<DynamicField> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            logger.LogStartAction(tuple, data, parent);

            if (HasChildren(tuple.Local) || HasChildren(tuple.Remote) || HasChildren(tuple.Synchronized))
            {
                logger.LogTrace("Dynamic field has children");
                logger.LogBeforeMerge(tuple);
                tuple.Merge();
                logger.LogAfterMerge(tuple);
                var id1 = tuple.Local?.ID ?? 0;
                var id2 = tuple.Synchronized?.ID ?? 0;
                logger.LogDebug("Local id = {@Local}, Synchronized id = {@Synchronized}", id1, id2);
                var results = await getSubstrategy().Synchronize(
                    data,
                    connectionContext,
                    tuple.Remote?.ChildrenDynamicFields?.ToList() ?? new List<DynamicField>(),
                    token,
                    field => field.ParentFieldID == id1 || field.ParentFieldID == id2 ||
                        (field.SynchronizationMate != null &&
                            (field.SynchronizationMate.ParentFieldID == id1 ||
                                field.SynchronizationMate.ParentFieldID == id2)),
                    null,
                    tuple);
                logger.LogTrace("Children synchronized by substrategy");
                ((ISynchronizationChanges)parent).SynchronizeChanges(tuple);
                return results;
            }

            return null;
        }

        private bool HasChildren(DynamicField field)
            => (field?.ChildrenDynamicFields?.Count ?? 0) != 0;

        private class DynamicFieldComparer : IEqualityComparer<DynamicField>
        {
            public bool Equals(DynamicField x, DynamicField y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (ReferenceEquals(x, null))
                    return false;

                if (ReferenceEquals(y, null))
                    return false;

                if (x.GetType() != y.GetType())
                    return false;

                if (x.ExternalID == y.ExternalID)
                    return true;

                if (!string.IsNullOrWhiteSpace(x.ExternalID) && !string.IsNullOrWhiteSpace(y.ExternalID))
                    return false;

                var xFields = x.ChildrenDynamicFields ?? Enumerable.Empty<DynamicField>();
                var yFields = y.ChildrenDynamicFields ?? Enumerable.Empty<DynamicField>();
                return
                    x.Type == y.Type &&
                    x.Name == y.Name &&
                    x.Value == y.Value &&
                    (x.ChildrenDynamicFields?.Count ?? 0) == (y.ChildrenDynamicFields?.Count ?? 0) &&
                    xFields.All(xc => yFields.Any(yc => this.Equals(xc, yc)));
            }

            public int GetHashCode(DynamicField obj)
                => 0;
        }
    }
}
