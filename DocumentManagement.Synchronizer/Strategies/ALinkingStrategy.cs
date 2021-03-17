using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Synchronization.Extensions;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal abstract class ALinkingStrategy<TDB, TDto> : ASynchronizationStrategy<TDB, TDto>
        where TDB : class, ISynchronizable<TDB>, new()
    {
        private readonly LinkingFunc link;
        private readonly LinkingFunc update;
        private readonly LinkingFunc unlink;

        protected ALinkingStrategy(
            IMapper mapper,
            LinkingFunc link,
            LinkingFunc update,
            LinkingFunc unlink)
            : base(mapper)
        {
            this.link = link;
            this.update = update;
            this.unlink = unlink;
        }

        public delegate Task LinkingFunc(DMContext context, TDB item, object parent, EntityType entityType);

        protected abstract override DbSet<TDB> GetDBSet(DMContext context);

        protected override ISynchronizer<TDto> GetSynchronizer(IConnectionContext context)
            => throw new WarningException($"Updating {typeof(TDB).Name} must be in parent synchronizer");

        protected override Expression<Func<TDB, bool>> GetDefaultFilter(SynchronizingData data)
            => x => true;

        protected override async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                tuple.Merge();
                await link(data.Context, tuple.Local, parent, EntityType.Local);
                await link(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
                return null;
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

        protected override async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                tuple.Merge();
                await link(data.Context, tuple.Remote, parent, EntityType.Remote);
                await link(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
                return null;
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

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                tuple.Merge();
                if (tuple.LocalChanged)
                    await update(data.Context, tuple.Local, parent, EntityType.Local);
                if (tuple.SynchronizedChanged)
                    await update(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
                if (tuple.RemoteChanged)
                    await update(data.Context, tuple.Remote, parent, EntityType.Remote);
                return null;
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
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                if (tuple.Remote != null)
                    await unlink(data.Context, tuple.Remote, parent, EntityType.Remote);
                if (tuple.Synchronized != null)
                    await unlink(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
                return null;
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

        protected override async Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            try
            {
                if (tuple.Local != null)
                    await unlink(data.Context, tuple.Local, parent, EntityType.Local);
                if (tuple.Synchronized != null)
                    await unlink(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
                return null;
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
    }
}
