using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Mergers
{
    internal class ObjectiveMerger : IMerger<Objective>
    {
        private readonly Lazy<IChildrenMerger<Objective, BimElement>> bimElementChildrenMerger;
        private readonly Lazy<IChildrenMerger<Objective, DynamicField>> dynamicFieldChildrenMerger;
        private readonly Lazy<IChildrenMerger<Objective, Item>> itemChildrenMerger;
        private readonly IMerger<Location> locationMerger;
        private readonly ILogger<ObjectiveMerger> logger;

        public ObjectiveMerger(
            ILogger<ObjectiveMerger> logger,
            IMerger<Location> locationMerger,
            IFactory<IChildrenMerger<Objective, DynamicField>> dynamicFieldChildrenMerger,
            IFactory<IChildrenMerger<Objective, Item>> itemChildrenMerger,
            IFactory<IChildrenMerger<Objective, BimElement>> bimElementChildrenMerger)
        {
            this.logger = logger;
            this.locationMerger = locationMerger;
            this.dynamicFieldChildrenMerger = new Lazy<IChildrenMerger<Objective, DynamicField>>(dynamicFieldChildrenMerger.Create);
            this.itemChildrenMerger = new Lazy<IChildrenMerger<Objective, Item>>(itemChildrenMerger.Create);
            this.bimElementChildrenMerger =
                new Lazy<IChildrenMerger<Objective, BimElement>>(bimElementChildrenMerger.Create);
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
            await dynamicFieldChildrenMerger.Value.MergeChildren(tuple).ConfigureAwait(false);
            await itemChildrenMerger.Value.MergeChildren(tuple).ConfigureAwait(false);
            await bimElementChildrenMerger.Value.MergeChildren(tuple).ConfigureAwait(false);
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
