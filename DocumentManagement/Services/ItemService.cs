﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Services;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface;

namespace MRS.DocumentManagement.Services
{
    public class ItemService : IItemService
    {
        private readonly DMContext context;

        public ItemService(DMContext context)
        {
            this.context = context;
        }

        public async Task<ID<ItemDto>> Add(ItemToCreateDto data, ID<ProjectDto> parentProject)
        {
            var item = new Database.Models.Item() 
            {
                Name = data.Name,
                ItemType = (int)data.ItemType
            };
            context.Items.Add(item);
            await context.SaveChangesAsync();

            await Link((ID<ItemDto>)item.ID, parentProject);
            return (ID<ItemDto>)item.ID;
        }

        public async Task<ID<ItemDto>> Add(ItemToCreateDto data, ID<ObjectiveDto> parentObjective)
        {
            var item = new Database.Models.Item()
            {
                Name = data.Name,
                ItemType = (int)data.ItemType
            };
            context.Items.Add(item);
            await context.SaveChangesAsync();

            await Link((ID<ItemDto>)item.ID, parentObjective);
            return (ID<ItemDto>)item.ID;
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

        public async Task<bool> Link(ID<ItemDto> itemID, ID<ProjectDto> projectID)
        {
            var isLinked = await context.ProjectItems.Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ProjectID == (int)projectID)
                .AnyAsync();
            
            if (isLinked)
                return false;

            await context.ProjectItems.AddAsync(new Database.Models.ProjectItem()
            {
                ItemID = (int)itemID,
                ProjectID = (int)projectID
            });
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Link(ID<ItemDto> itemID, ID<ObjectiveDto> objectiveID)
        {
            var isLinked = await context.ObjectiveItems.Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .AnyAsync();

            if (isLinked)
                return false;

            await context.ObjectiveItems.AddAsync(new Database.Models.ObjectiveItem()
            {
                ItemID = (int)itemID,
                ObjectiveID = (int)objectiveID
            });
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Unlink(ID<ItemDto> itemID, ID<ProjectDto> projectID)
        {
            var link = await context.ProjectItems
                .Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ProjectID == (int)projectID)
                .FirstOrDefaultAsync();
            if (link == null)
                return false;
            context.ProjectItems.Remove(link);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Unlink(ID<ItemDto> itemID, ID<ObjectiveDto> objectiveID)
        {
            var link = await context.ObjectiveItems
                .Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .FirstOrDefaultAsync();
            if (link == null)
                return false;
            context.ObjectiveItems.Remove(link);
            await context.SaveChangesAsync();
            return true;
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
