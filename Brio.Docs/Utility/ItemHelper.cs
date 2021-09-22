using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility
{
    public class ItemHelper
    {
        private readonly ILogger<ItemHelper> logger;

        public ItemHelper(ILogger<ItemHelper> logger)
            => this.logger = logger;

        public async Task<Item> CheckItemToLink<TParent>(DMContext context, IMapper mapper, ItemDto item, TParent parent)
        {
            logger.LogTrace("CheckItemToLink started with item: {@Item}, parent: {@Parent}", item, parent);
            var dbItem = await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.ID == (int)item.ID) ??
                         await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.RelativePath == item.RelativePath);
            logger.LogDebug("Found item: {@Item}", dbItem);

            if (await ShouldCreateNewItem(dbItem, parent, context))
            {
                logger.LogDebug("Should create new item");
                dbItem = mapper.Map<Item>(item);
                logger.LogDebug("Mapped item: {@Item}", dbItem);
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

            logger.LogDebug("Already linked: {IsLinked}", alreadyLinked);
            return alreadyLinked ? null : dbItem;
        }

        private async Task<bool> ShouldCreateNewItem<TParent>(Item dbItem, TParent parent, DMContext context)
        {
            logger.LogTrace("ShouldCreateNewItem started with item: {@Item}, parent: {@Parent}", dbItem, parent);

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
