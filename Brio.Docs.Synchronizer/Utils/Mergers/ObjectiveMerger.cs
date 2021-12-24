using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Mergers
{
    internal class ObjectiveMerger : IMerger<Objective>
    {
        private readonly IChildrenMerger<Objective, BimElement> bimElementChildrenMerger;
        private readonly IChildrenMerger<Objective, DynamicField> dynamicFieldChildrenMerger;
        private readonly IChildrenMerger<Objective, Item> itemChildrenMerger;
        private readonly IMerger<Location> locationMerger;
        private readonly ILogger<ObjectiveMerger> logger;

        public ObjectiveMerger(
            ILogger<ObjectiveMerger> logger,
            IMerger<Location> locationMerger,
            IChildrenMerger<Objective, DynamicField> dynamicFieldChildrenMerger,
            IChildrenMerger<Objective, Item> itemChildrenMerger,
            IChildrenMerger<Objective, BimElement> bimElementChildrenMerger)
        {
            this.logger = logger;
            this.locationMerger = locationMerger;
            this.dynamicFieldChildrenMerger = dynamicFieldChildrenMerger;
            this.itemChildrenMerger = itemChildrenMerger;
            this.bimElementChildrenMerger = bimElementChildrenMerger;
        }

        public async ValueTask Merge(SynchronizingTuple<Objective> tuple)
        {
            tuple.Merge(
                objective => objective.Author,
                objective => objective.AuthorID,
                objective => objective.CreationDate,
                objective => objective.DueDate,
                objective => objective.Title,
                objective => objective.TitleToLower,
                objective => objective.Description,
                objective => objective.Status);

            await MergeLocation(tuple).ConfigureAwait(false);
            await dynamicFieldChildrenMerger.MergeChildren(tuple).ConfigureAwait(false);
            await itemChildrenMerger.MergeChildren(tuple).ConfigureAwait(false);
            await bimElementChildrenMerger.MergeChildren(tuple).ConfigureAwait(false);
        }

        private async ValueTask MergeLocation(SynchronizingTuple<Objective> tuple)
        {
            var locationTuple = new SynchronizingTuple<Location>(
                null,
                tuple.Local.Location,
                tuple.Synchronized.Location,
                tuple.Remote.Location);

            var item = locationTuple.Remote?.Item;

            if (item is { ProjectID: null, Objectives: null })
            {
                item.Objectives = new List<ObjectiveItem>
                {
                    new ()
                    {
                        Objective = tuple.Remote ?? tuple.Synchronized ?? tuple.Local,
                    },
                };
            }

            await locationMerger.Merge(locationTuple).ConfigureAwait(false);
        }
    }
}
