using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ItemHelper
    {
        private readonly ILogger<ItemHelper> logger;

        public ItemHelper(ILogger<ItemHelper> logger)
            => this.logger = logger;

        public async Task<Item> CheckItemToLink(DMContext context, IMapper mapper, ItemDto item, Type itemParentType, int parentId)
        {
            logger.LogTrace(
                "CheckItemToLink started with item: {@Item}, itemParentType: {@Type}, parentId: {@ParentID}",
                item,
                itemParentType,
                parentId);
            var dbItem = await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.ID == (int)item.ID) ??
                         await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.RelativePath == item.RelativePath);
            logger.LogDebug("Found item: {@Item}", dbItem);

            if (await ShouldCreateNewItem(dbItem, itemParentType, parentId, context))
            {
                logger.LogDebug("Should create new item");
                dbItem = mapper.Map<Item>(item);
                logger.LogDebug("Mapped item: {@Item}", dbItem);
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

            logger.LogDebug("Already linked: {IsLinked}", alreadyLinked);
            return alreadyLinked ? null : dbItem;
        }

        private async Task<bool> ShouldCreateNewItem(Item dbItem, Type itemParentType, int parentId, DMContext context)
        {
            logger.LogTrace(
                "ShouldCreateNewItem started with item: {@Item}, itemParentType: {@Type}, parentId: {@ParentID}",
                dbItem,
                itemParentType,
                parentId);

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
            logger.LogDebug("Found item: {@Item}", item);

            // Check if same item exists (linked to same project)
            if (item != default)
                return false;

            item = await context.ObjectiveItems
                .Where(x => x.Objective.ProjectID == projectID)
                .Select(x => x.Item)
                .FirstOrDefaultAsync(x => x == dbItem);
            logger.LogDebug("Found item: {@Item}", item);

            // Check if same item exists (linked to any objectives in same project)
            return item == default;
        }
    }
}
