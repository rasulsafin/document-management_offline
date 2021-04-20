using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Utils.Linkers
{
    internal class ObjectiveItemLinker : AItemLinker
    {
        public override Task Link(DMContext context, Item item, object parent, EntityType entityType)
        {
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
                    new ObjectiveItem
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
            var objective = LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);

            if (entityType == EntityType.Remote)
            {
                objective.Items.Remove(objective.Items.First(x => Equals(x.Item, item)));
                return Task.CompletedTask;
            }

            item.Objectives.Remove(item.Objectives.First(x => Equals(x.Objective, objective)));
            if (item.Project == null && item.Objectives?.Count == 0)
                context.Items.Remove(item);
            return Task.CompletedTask;
        }
    }
}
