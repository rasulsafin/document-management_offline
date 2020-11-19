using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DocumentManagement.Database;
using DocumentManagement.Interface.Models;
using DocumentManagement.Interface.Services;
using System.Linq;

namespace DocumentManagement.Services
{
    internal class ItemService : IItemService
    {
        private readonly DMContext context;

        public ItemService(DMContext context)
        {
            this.context = context;
        }

        public async Task<ID<Item>> Add(NewItem data, ID<Project> parentProject)
        {
            var item = new Database.Models.Item() 
            {
                Path = data.Path,
                ItemType = (int)data.ItemType
            };
            context.Items.Add(item);
            await context.SaveChangesAsync();

            await Link((ID<Item>)item.ID, parentProject);
            return (ID<Item>)item.ID;
        }

        public async Task<ID<Item>> Add(NewItem data, ID<Objective> parentObjective)
        {
            var item = new Database.Models.Item()
            {
                Path = data.Path,
                ItemType = (int)data.ItemType
            };
            context.Items.Add(item);
            await context.SaveChangesAsync();

            await Link((ID<Item>)item.ID, parentObjective);
            return (ID<Item>)item.ID;
        }

        public async Task<Item> Find(ID<Item> itemID)
        {
            var dbItem = await context.Items.FindAsync((int)itemID);
            if (dbItem == null)
                return null;
            return MapItemFromDB(dbItem);
        }

        public async Task<Item> Find(string path)
        {
            var dbItem = await context.Items.FirstOrDefaultAsync(x => x.Path == path);
            if (dbItem == null)
                return null;
            return MapItemFromDB(dbItem);
        }

        public async Task<IEnumerable<Item>> GetItems(ID<Project> projectID)
        {
            var dbItems = await context.ProjectItems
                .Where(x => x.ProjectID == (int)projectID)
                .Select(x => x.Item)
                .ToListAsync();
            return dbItems.Select(x => MapItemFromDB(x)).ToList();
        }

        public async Task<IEnumerable<Item>> GetItems(ID<Objective> objectiveID)
        {
            var dbItems = await context.ObjectiveItems
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .Select(x => x.Item)
                .ToListAsync();
            return dbItems.Select(x => MapItemFromDB(x)).ToList();
        }

        public async Task Link(ID<Item> itemID, ID<Project> projectID)
        {
            var isLinked = await context.ProjectItems.Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ProjectID == (int)projectID)
                .AnyAsync();
            
            if (isLinked)
                return;

            await context.ProjectItems.AddAsync(new Database.Models.ProjectItem()
            {
                ItemID = (int)itemID,
                ProjectID = (int)projectID
            });
            await context.SaveChangesAsync();
        }

        public async Task Link(ID<Item> itemID, ID<Objective> objectiveID)
        {
            var isLinked = await context.ObjectiveItems.Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .AnyAsync();

            if (isLinked)
                return;

            await context.ObjectiveItems.AddAsync(new Database.Models.ObjectiveItem()
            {
                ItemID = (int)itemID,
                ObjectiveID = (int)objectiveID
            });
            await context.SaveChangesAsync();
        }

        public async Task Unlink(ID<Item> itemID, ID<Project> projectID)
        {
            var link = await context.ProjectItems
                .Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ProjectID == (int)projectID)
                .FirstOrDefaultAsync();
            if (link == null)
                return;
            context.ProjectItems.Remove(link);
            await context.SaveChangesAsync();
        }

        public async Task Unlink(ID<Item> itemID, ID<Objective> objectiveID)
        {
            var link = await context.ObjectiveItems
                .Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .FirstOrDefaultAsync();
            if (link == null)
                return;
            context.ObjectiveItems.Remove(link);
            await context.SaveChangesAsync();
        }

        public async Task Update(Item item)
        {
            var dbItem = await context.Items.FindAsync((int)item.ID);
            if (dbItem == null)
                throw new ArgumentException($"Item with key {item.ID} not found");
            dbItem.ItemType = (int)item.ItemType;
            dbItem.Path = item.Path;
            context.Items.Update(dbItem);
            await context.SaveChangesAsync();
        }

        private static Item MapItemFromDB(Database.Models.Item dbItem)
        {
            return new Item()
            {
                ID = (ID<Item>)dbItem.ID,
                ItemType = (ItemType)dbItem.ItemType,
                Path = dbItem.Path
            };
        }
    }
}
