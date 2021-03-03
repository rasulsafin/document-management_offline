using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

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
                await context.Items.AddAsync(dbItem);
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
                    alreadyLinked = dbItem.ProjectID == parentId;
                    break;
            }

            return alreadyLinked ? null : dbItem;
        }
    }
}
