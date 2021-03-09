using System;
using System.ComponentModel;
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

namespace MRS.DocumentManagement.Synchronization.Strategies
{
    internal class ItemStrategy : ASynchronizationStrategy<Item, ItemExternalDto>
    {
        private readonly LinkingFunc link;

        private readonly LinkingFunc unlink;

        public ItemStrategy(
            IMapper mapper,
            LinkingFunc link,
            LinkingFunc unlink)
            : base(mapper)
        {
            this.link = link;
            this.unlink = unlink;
        }

        public delegate Task LinkingFunc(DMContext context, Item item, object parent, EntityType entityType);

        protected override DbSet<Item> GetDBSet(DMContext context)
            => context.Items;

        protected override ISynchronizer<ItemExternalDto> GetSynchronizer(IConnectionContext context)
            => throw new WarningException("Updating remote items must be in parent synchronizer");

        protected override IIncludableQueryable<Item, Item> Include(IQueryable<Item> set)
            => base.Include(
                set.Include(x => x.Objectives)
                   .Include(x => x.Project));

        protected override Expression<Func<Item, bool>> GetDefaultFilter(SynchronizingData data)
            => x => true;

        protected override async Task AddToLocal(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            tuple.Merge();
            await link(data.Context, tuple.Local, parent, EntityType.Local);
            await link(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
        }

        protected override async Task AddToRemote(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            tuple.Merge();
            await link(data.Context, tuple.Remote, parent, EntityType.Remote);
            await link(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
        }

        protected override async Task<SynchronizingResult> Merge(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            await NothingAction(tuple, data, connectionContext, parent);
            return new SynchronizingResult { Status = SynchronizingStatus.Successful };
        }

        protected override async Task RemoveFromRemote(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            if (tuple.Remote != null)
                await unlink(data.Context, tuple.Remote, parent, EntityType.Remote);
            if (tuple.Synchronized != null)
                await unlink(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
        }

        protected override async Task RemoveFromLocal(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            object parent)
        {
            if (tuple.Local != null)
                await unlink(data.Context, tuple.Local, parent, EntityType.Local);
            if (tuple.Synchronized != null)
                await unlink(data.Context, tuple.Synchronized, parent, EntityType.Synchronized);
        }

        protected override bool IsEntitiesEquals(Item element, SynchronizingTuple<Item> tuple)
            => base.IsEntitiesEquals(element, tuple) ||
                element.RelativePath == (string)tuple.GetPropertyValue(nameof(Item.RelativePath));
    }
}
