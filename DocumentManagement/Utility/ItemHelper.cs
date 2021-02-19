using AutoMapper;
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
        public async Task<Item> CheckItemToLink(DMContext context, IMapper mapper, ItemDto item, Type itemParentType, int parentId)
        {
            var dbItem = await context.Items
                    .FirstOrDefaultAsync(i => i.ID == (int)item.ID);

            if (dbItem == null)
            {
                dbItem = mapper.Map<Item>(item);
                context.Items.Add(dbItem);
                await context.SaveChangesAsync();
                return dbItem;
            }

            bool alreadyLinked = false;

            switch (itemParentType)
            {
                case var _ when itemParentType == typeof(Objective):
                    alreadyLinked = await context.ObjectiveItems
                        .AnyAsync(i => i.ItemID == (int)item.ID && i.ObjectiveID == parentId);
                    break;
                case var _ when itemParentType == typeof(Project):
                    alreadyLinked = await context.ProjectItems
                        .AnyAsync(i => i.ItemID == (int)item.ID && i.ProjectID == parentId);
                    break;
            }

            if (alreadyLinked)
                return null;

            return dbItem;
        }
    }
}
