using System;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers
{
    internal class LocationMerger : IMerger<Location>
    {
        private readonly DMContext context;
        private readonly IAttacher<Item> itemAttacher;
        private readonly ILogger<LocationMerger> logger;

        public LocationMerger(
            DMContext context,
            ILogger<LocationMerger> logger,
            IAttacher<Item> itemAttacher)
        {
            this.context = context;
            this.logger = logger;
            this.itemAttacher = itemAttacher;
            logger.LogTrace("LocationMerger created");
        }

        public async ValueTask Merge(SynchronizingTuple<Location> tuple)
        {
            logger.LogTrace(
                "Merge location started for tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.ID,
                tuple.Synchronized?.ID,
                tuple.ExternalID);
            tuple.Merge(
                await GetUpdatedTime(tuple.Local).ConfigureAwait(false),
                await GetUpdatedTime(tuple.Remote).ConfigureAwait(false),
                location => location.PositionX,
                location => location.PositionY,
                location => location.PositionZ,
                location => location.CameraPositionX,
                location => location.CameraPositionY,
                location => location.CameraPositionZ,
                location => location.Guid);

            logger.LogAfterMerge(tuple);
            await LinkLocationItem(tuple).ConfigureAwait(false);
            logger.LogTrace("Location item merged");
        }

        private void CreateRemoteLocationItem(SynchronizingTuple<Location> tuple, SynchronizingTuple<Item> itemTuple)
        {
            logger.LogDebug("Creating remote");

            itemTuple.Remote = new Item
            {
                ExternalID = itemTuple.Synchronized.ExternalID,
                ItemType = itemTuple.Synchronized.ItemType,
                RelativePath = itemTuple.Synchronized.RelativePath,
                ProjectID = itemTuple.Synchronized.ProjectID,
            };
            logger.LogDebug("Created item: {@Object}", tuple.Local);
            itemTuple.RemoteChanged = true;
        }

        private async ValueTask<DateTime> GetUpdatedTime(Location location)
        {
            if (location == null)
                return default;

            if (location.Objective != null)
                return location.Objective.UpdatedAt;

            if (location.ID == 0)
                return default;

            return await context.Set<Location>()
               .AsNoTracking()
               .Where(x => x.ID == location.ID)
               .Select(x => x.Objective.UpdatedAt)
               .FirstOrDefaultAsync()
               .ConfigureAwait(false);
        }

        private async Task LinkLocationItem(SynchronizingTuple<Location> tuple)
        {
            logger.LogTrace("LinkLocationItem started with {@Tuple}", tuple);

            var itemTuple = new SynchronizingTuple<Item>(
                local: tuple.Local.Item,
                synchronized: tuple.Synchronized.Item,
                remote: tuple.Remote.Item);

            var idsTuple = new SynchronizingTuple<string>(
                local: itemTuple.Local?.ExternalID,
                synchronized: itemTuple.Synchronized?.ExternalID,
                remote: itemTuple.Remote?.ExternalID);

            var action = idsTuple.DetermineAction();

            if (action == SynchronizingAction.Merge)
            {
                var relevantId = idsTuple.GetRelevant(
                    await GetUpdatedTime(tuple.Local).ConfigureAwait(false),
                    await GetUpdatedTime(tuple.Remote).ConfigureAwait(false));

                itemTuple.RemoveWhere(x => x?.ExternalID != relevantId);
                itemTuple.ExternalID = relevantId;
            }

            await itemAttacher.AttachExisting(itemTuple).ConfigureAwait(false);
            itemTuple.Synchronized ??= itemTuple.Local?.SynchronizationMate;

            if (itemTuple.Synchronized != null && itemTuple.Remote == null)
                CreateRemoteLocationItem(tuple, itemTuple);

            tuple.ForEachChange(
                itemTuple,
                (location, item) =>
                {
                    if (location.Item != item)
                    {
                        location.Item = item;
                        return true;
                    }

                    return false;
                });
        }
    }
}
