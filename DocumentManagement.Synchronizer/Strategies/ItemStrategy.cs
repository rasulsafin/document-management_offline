using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronizer.Extensions;
using MRS.DocumentManagement.Synchronizer.Models;

namespace MRS.DocumentManagement.Synchronizer.Strategies
{
    internal class ItemStrategy : ASynchronizationStrategy<Item, ItemExternalDto>
    {
        private readonly Func<Item, object, SynchronizingData, Task> link;
        private readonly Func<Item, object, SynchronizingData, Task> unlink;

        public ItemStrategy(
            IMapper mapper,
            Func<Item, object, SynchronizingData, Task> link,
            Func<Item, object, SynchronizingData, Task> unlink)
            : base(mapper)
        {
            this.link = link;
            this.unlink = unlink;
        }

        protected override DbSet<Item> GetDBSet(DMContext context)
            => context.Items;

        protected override ISynchronizer<ItemExternalDto> GetSynchronizer(AConnectionContext context)
            => throw new WarningException("Updating remote items must be in parent synchronizer");

        protected override IIncludableQueryable<Item, Item> Include(IQueryable<Item> set)
            => base.Include(set.Include(x => x.Objectives).Include(x => x.Project));

        protected override bool DefaultFilter(SynchronizingData data, Item item)
            => true;

        protected override async Task AddToRemote(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
        {
            tuple.Merge();
            await link(tuple.Local, parent, data);
            await link(tuple.Synchronized, parent, data);
        }

        protected override async Task Merge(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
            => await NothingAction(tuple, data, connectionContext, parent);

        protected override async Task RemoveFromRemote(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
            => await NothingAction(tuple, data, connectionContext, parent);

        protected override async Task RemoveFromLocal(
            SynchronizingTuple<Item> tuple,
            SynchronizingData data,
            AConnectionContext connectionContext,
            object parent)
        {
            if (tuple.Local != null)
                await unlink(tuple.Local, parent, data);
            if (tuple.Synchronized != null)
                await unlink(tuple.Synchronized, parent, data);
        }

        protected override bool IsEntitiesEquals(Item element, SynchronizingTuple<Item> tuple)
            => base.IsEntitiesEquals(element, tuple) ||
                element.Name == (string)tuple.GetPropertyValue(nameof(Item.Name));
    }
}
