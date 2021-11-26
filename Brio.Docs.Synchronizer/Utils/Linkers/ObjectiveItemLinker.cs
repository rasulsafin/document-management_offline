using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utils.Linkers
{
    internal class ObjectiveItemLinker : AItemLinker
    {
        private readonly ILogger<ObjectiveItemLinker> logger;

        public ObjectiveItemLinker(ILogger<ObjectiveItemLinker> logger)
        {
            this.logger = logger;
            logger.LogTrace("ObjectiveItemLinker created");
        }

        public override Task Link(DMContext context, Item item, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Link started with item: {@Item}, parent: {@Parent}, entityType: {@EntityType}",
                item,
                parent,
                entityType);
            var objective = LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);
            objective.Items ??= new List<ObjectiveItem>();
            objective.Items.Add(
                new ObjectiveItem
                {
                    Item = item,
                    ObjectiveID = objective.ID,
                });

            if (entityType == EntityType.Remote)
            {
                item.Objectives ??= new List<ObjectiveItem>
                {
                    new ()
                    {
                        Item = item,
                        ObjectiveID = objective.ID,
                        Objective = objective,
                    },
                };
            }

            return Task.CompletedTask;
        }

        public override Task Unlink(DMContext context, Item item, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Unlink started with item: {@Item}, parent: {@Parent}, entityType: {@EntityType}",
                item,
                parent,
                entityType);
            var objective = LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);

            if (entityType == EntityType.Remote)
            {
                objective.Items.Remove(objective.Items.First(x => Equals(x.Item, item)));
                return Task.CompletedTask;
            }

            item.Objectives.Remove(item.Objectives.First(x => Equals(x.ObjectiveID, objective.ID)));
            if (item.Project == null && item.Objectives?.Count == 0)
                context.Items.Remove(item);
            return Task.CompletedTask;
        }
    }
}
