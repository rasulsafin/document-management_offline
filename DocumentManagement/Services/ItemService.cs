using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Services
{
    public class ItemService : IItemService
    {
        private readonly DMContext context;

        public ItemService(DMContext context)
        {
            this.context = context;
        }

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            var dbItem = await context.Items.FindAsync((int)itemID);
            if (dbItem == null)
                return null;
            return MapItemFromDB(dbItem);
        }
        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            var dbItems = await context.ProjectItems
                .Where(x => x.ProjectID == (int)projectID)
                .Select(x => x.Item)
                .ToListAsync();
            return dbItems.Select(x => MapItemFromDB(x)).ToList();
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            var dbItems = await context.ObjectiveItems
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .Select(x => x.Item)
                .ToListAsync();
            return dbItems.Select(x => MapItemFromDB(x)).ToList();
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

        private static ItemDto MapItemFromDB(Database.Models.Item dbItem)
        {
            return new ItemDto()
            {
                ID = (ID<ItemDto>)dbItem.ID,
                ItemType = (ItemTypeDto)dbItem.ItemType,
                Name = dbItem.Name
            };
        }
    }
}
