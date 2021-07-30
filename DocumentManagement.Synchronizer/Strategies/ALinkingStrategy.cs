using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Synchronization.Extensions;
using MRS.DocumentManagement.Synchronization.Interfaces;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal abstract class ALinkingStrategy<TDB, TDto> : ASynchronizationStrategy<TDB, TDto>
        where TDB : class, ISynchronizable<TDB>, new()
    {
        private readonly ILinker<TDB> linker;

        protected ALinkingStrategy(
            DMContext context,
            IMapper mapper,
            ILinker<TDB> linker)
            : base(context, mapper, false)
            => this.linker = linker;

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
            try
            {
                tuple.Merge();
                await linker.Link(context, tuple.Local, parent, EntityType.Local);
                await linker.Link(context, tuple.Synchronized, parent, EntityType.Synchronized);
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
            object parent,
            CancellationToken token)
        {
            try
            {
                tuple.Merge();
                await linker.Link(context, tuple.Remote, parent, EntityType.Remote);
                await linker.Link(context, tuple.Synchronized, parent, EntityType.Synchronized);
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
            object parent,
            CancellationToken token)
        {
            try
            {
                tuple.Merge();
                if (tuple.LocalChanged)
                    await linker.Update(context, tuple.Local, parent, EntityType.Local);
                if (tuple.SynchronizedChanged)
                    await linker.Update(context, tuple.Synchronized, parent, EntityType.Synchronized);
                if (tuple.RemoteChanged)
                    await linker.Update(context, tuple.Remote, parent, EntityType.Remote);
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
            object parent,
            CancellationToken token)
        {
            try
            {
                if (tuple.Remote != null)
                    await linker.Unlink(context, tuple.Remote, parent, EntityType.Remote);
                if (tuple.Synchronized != null)
                    await linker.Unlink(context, tuple.Synchronized, parent, EntityType.Synchronized);
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
            object parent,
            CancellationToken token)
        {
            try
            {
                if (tuple.Local != null)
                    await linker.Unlink(context, tuple.Local, parent, EntityType.Local);
                if (tuple.Synchronized != null)
                    await linker.Unlink(context, tuple.Synchronized, parent, EntityType.Synchronized);
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