using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal abstract class ALinkingStrategy<TDB, TDto> : ASynchronizationStrategy<TDB, TDto>
        where TDB : class, ISynchronizable<TDB>, new()
    {
        private readonly ILinker<TDB> linker;
        private readonly ILogger<ALinkingStrategy<TDB, TDto>> logger;

        protected ALinkingStrategy(
            DMContext context,
            IMapper mapper,
            ILinker<TDB> linker,
            ILogger<ALinkingStrategy<TDB, TDto>> logger)
            : base(context, mapper, logger, false)
        {
            this.linker = linker;
            this.logger = logger;
            logger.LogTrace("ALinkingStrategy created");
        }

        protected abstract override DbSet<TDB> GetDBSet(DMContext context);

        protected override ISynchronizer<TDto> GetSynchronizer(IConnectionContext context)
            => throw new WarningException($"Updating {typeof(TDB).Name} must be in parent synchronizer");

        protected override Expression<Func<TDB, bool>> GetDefaultFilter(SynchronizingData data)
            => x => true;

        protected override async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<TDB> tuple,
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
                await linker.Link(context, tuple.Local, parent, EntityType.Local);
                logger.LogDebug("Local {@Object} linked", tuple.Local);
                await linker.Link(context, tuple.Synchronized, parent, EntityType.Synchronized);
                logger.LogDebug("Synchronized {@Object} linked", tuple.Synchronized);
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

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<TDB> tuple,
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
                await linker.Link(context, tuple.Remote, parent, EntityType.Remote);
                logger.LogDebug("Remote {@Object} linked", tuple.Remote);
                await linker.Link(context, tuple.Synchronized, parent, EntityType.Synchronized);
                logger.LogDebug("Synchronized {@Object} linked", tuple.Synchronized);
                return null;
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
            SynchronizingTuple<TDB> tuple,
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

                if (tuple.LocalChanged)
                {
                    await linker.Update(context, tuple.Local, parent, EntityType.Local);
                    logger.LogDebug("Local {@Object} updated", tuple.Local);
                }

                if (tuple.SynchronizedChanged)
                {
                    await linker.Update(context, tuple.Synchronized, parent, EntityType.Synchronized);
                    logger.LogDebug("Synchronized {@Object} updated", tuple.Synchronized);
                }

                if (tuple.RemoteChanged)
                {
                    await linker.Update(context, tuple.Remote, parent, EntityType.Remote);
                    logger.LogDebug("Remote {@Object} updated", tuple.Remote);
                }

                return null;
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

        protected override async Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                if (tuple.Remote != null)
                {
                    await linker.Unlink(context, tuple.Remote, parent, EntityType.Remote);
                    logger.LogDebug("Remote {@Object} unlinked", tuple.Remote);
                }

                if (tuple.Synchronized != null)
                {
                    await linker.Unlink(context, tuple.Synchronized, parent, EntityType.Synchronized);
                    logger.LogDebug("Synchronized {@Object} unlinked", tuple.Synchronized);
                }

                return null;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromRemote, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Synchronized,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        protected override async Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogStartAction(tuple, data, parent);

            try
            {
                if (tuple.Local != null)
                {
                    await linker.Unlink(context, tuple.Local, parent, EntityType.Local);
                    logger.LogDebug("Local {@Object} unlinked", tuple.Local);
                }

                if (tuple.Synchronized != null)
                {
                    await linker.Unlink(context, tuple.Synchronized, parent, EntityType.Synchronized);
                    logger.LogDebug("Synchronized {@Object} unlinked", tuple.Synchronized);
                }

                return null;
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
    }
}
