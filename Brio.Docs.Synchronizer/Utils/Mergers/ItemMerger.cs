using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Mergers
{
    internal class ItemMerger : IMerger<Item>
    {
        private readonly ILogger<ItemMerger> logger;

        public ItemMerger(ILogger<ItemMerger> logger)
        {
            this.logger = logger;
            logger.LogTrace("ItemMerger created");
        }

        public Task Merge(SynchronizingTuple<Item> tuple)
        {
            logger.LogTrace("Merge started for tuple {@Object}", tuple);
            tuple.Merge(
                item => item.RelativePath,
                item => item.ItemType);
            logger.LogDebug("Tuple merged: {@Result}", tuple);
            return Task.CompletedTask;
        }
    }
}
