using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        public ItemStrategy(IMapper mapper)
            : base(mapper)
        {
        }

        protected override DbSet<Item> GetDBSet(DMContext context)
            => context.Items;

        protected override ISynchronizer<ItemExternalDto> GetSynchronizer(AConnectionContext context)
            => context.ItemsSynchronizer;

        protected override bool DefaultFilter(SynchronizingData data, Item item)
            => true;

        protected override Task Merge(SynchronizingTuple<Item> tuple, SynchronizingData data, AConnectionContext connectionContext)
        {
            NothingAction(tuple, data, connectionContext);
            return Task.CompletedTask;
        }

        protected override bool IsEntitiesEquals(Item element, SynchronizingTuple<Item> tuple)
            => base.IsEntitiesEquals(element, tuple) ||
                element.Name == (string)tuple.GetPropertyValue(nameof(Item.Name));
    }
}
