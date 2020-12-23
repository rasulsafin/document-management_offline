using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace MRS.DocumentManagement.Services
{
    public class ItemService : IItemService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;

        public ItemService(DMContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            var dbItem = await context.Items.FindAsync((int)itemID);
            return dbItem == null ? null : MapItemFromDB(dbItem);
        }
        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            var dbItems = await context.ProjectItems
                .Where(x => x.ProjectID == (int)projectID)
                .Select(x => x.Item)
                .ToListAsync();
            return dbItems.Select(MapItemFromDB).ToList();
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            var dbItems = await context.ObjectiveItems
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .Select(x => x.Item)
                .ToListAsync();
            return dbItems.Select(MapItemFromDB).ToList();
        }

        public async Task<bool> Update(ItemDto item)
        {
            var dbItem = await context.Items.FindAsync((int)item.ID);
            if (dbItem == null)
                return false;

            dbItem.ItemType = (int)item.ItemType;
            dbItem.Name = item.Name;
            context.Items.Update(dbItem);
            await context.SaveChangesAsync();
            return true;
        }

        private ItemDto MapItemFromDB(Database.Models.Item dbItem) 
            => mapper.Map<ItemDto>(dbItem);
    }
}
