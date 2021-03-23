using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ItemHelper
    {
        public async Task<Item> CheckItemToLink(DMContext context, IMapper mapper, ItemDto item, Type itemParentType, int parentId)
        {
            var dbItem = await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.ID == (int)item.ID) ??
                         await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.RelativePath == item.RelativePath);

            if (await ShouldCreateNewItem(dbItem, itemParentType, parentId, context))
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

        private async Task<bool> ShouldCreateNewItem(Item dbItem, Type itemParentType, int parentId, DMContext context)
        {
            // Check if item exists
            if (dbItem == null)
                return true;

            int projectID = -1;
            switch (itemParentType)
            {
                case var _ when itemParentType == typeof(Objective):

                    var objective = await context.Objectives.FirstOrDefaultAsync(x => x.ID == parentId);
                    projectID = objective.ProjectID;
                    break;

                case var _ when itemParentType == typeof(Project):

                    projectID = parentId;
                    break;
            }

            var item = await context.Items
                .Unsynchronized()
                .FirstOrDefaultAsync(x => x.ProjectID == projectID && x.RelativePath == dbItem.RelativePath);

            // Check if same item exists (linked to same project)
            if (item != default)
                return false;

            item = await context.ObjectiveItems
                .Where(x => x.Objective.ProjectID == projectID)
                .Select(x => x.Item)
                .FirstOrDefaultAsync(x => x == dbItem);

            // Check if same item exists (linked to any objectives in same project)
            return item == default;
        }
    }
}
