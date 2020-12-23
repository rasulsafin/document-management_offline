using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Utility
{
    public class ItemHelper
    {
        public static async Task<Item> CheckItemToLink(DMContext context, ItemDto item, Type itemParentType, int parentId)
        {
            var dbItem = await context.Items
                    .FirstOrDefaultAsync(i => i.ID == (int)item.ID);

            bool alreadyLinked = false;

            switch (itemParentType)
            {
                case var o when itemParentType == typeof(Objective):
                    alreadyLinked = await context.ObjectiveItems
                        .AnyAsync(i => i.ItemID == (int)item.ID && i.ObjectiveID == parentId);
                    break;
                case var p when itemParentType == typeof(Project):
                    alreadyLinked = await context.ProjectItems
                        .AnyAsync(i => i.ItemID == (int)item.ID && i.ProjectID == parentId);
                    break;
            }

            if (alreadyLinked)
                return null;

            if (dbItem == null)
            {
                dbItem = new Item
                {
                    ID = (int)item.ID,
                    ItemType = (int)item.ItemType,
                    ExternalItemId = item.ExternalItemId
                };
                context.Items.Add(dbItem);
            }

            return dbItem;
        }
    }
}
