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
        public async Task<Item> CheckItemToLink<TParent>(DMContext context, IMapper mapper, ItemDto item, TParent parent)
        {
            var dbItem = await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.ID == (int)item.ID) ??
                         await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.RelativePath == item.RelativePath);

            if (await ShouldCreateNewItem(dbItem, parent, context))
            {
                dbItem = mapper.Map<Item>(item);
                await context.Items.AddAsync(dbItem);
                await context.SaveChangesAsync();
                return dbItem;
            }

            bool alreadyLinked = false;

            switch (parent)
            {
                case Objective objective:
                    var id = (int)item.ID;
                    var parentID = objective.ID;
                    alreadyLinked = await context.ObjectiveItems
                       .AnyAsync(i => i.ItemID == id && i.ObjectiveID == parentID);
                    break;
                case Project project:
                    alreadyLinked = dbItem.ProjectID == project.ID;
                    break;
            }

            return alreadyLinked ? null : dbItem;
        }

        private async Task<bool> ShouldCreateNewItem<TParent>(Item dbItem, TParent parent, DMContext context)
        {
            // Check if item exists
            if (dbItem == null)
                return true;

            int projectID = -1;
            switch (parent)
            {
                case Objective objective:
                    projectID = objective.ProjectID;
                    break;
                case Project project:
                    projectID = project.ID;
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
