using System;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers
{
    internal class ObjectiveMerger : IMerger<Objective>
    {
        private readonly Lazy<IChildrenMerger<Objective, BimElement>> bimElementChildrenMerger;
        private readonly Lazy<IChildrenMerger<Objective, DynamicField>> dynamicFieldChildrenMerger;
        private readonly Lazy<IChildrenMerger<Objective, Item>> itemChildrenMerger;
        private readonly IMerger<Location> locationMerger;
        private readonly DbContext context;
        private readonly ILogger<ObjectiveMerger> logger;

        public ObjectiveMerger(
            DMContext context,
            ILogger<ObjectiveMerger> logger,
            IMerger<Location> locationMerger,
            IFactory<IChildrenMerger<Objective, DynamicField>> dynamicFieldChildrenMergerFactory,
            IFactory<IChildrenMerger<Objective, Item>> itemChildrenMergerFactory,
            IFactory<IChildrenMerger<Objective, BimElement>> bimElementChildrenMergerFactory)
        {
            this.context = context;
            this.logger = logger;
            this.locationMerger = locationMerger;
            this.dynamicFieldChildrenMerger =
                new Lazy<IChildrenMerger<Objective, DynamicField>>(dynamicFieldChildrenMergerFactory.Create);
            this.itemChildrenMerger = new Lazy<IChildrenMerger<Objective, Item>>(itemChildrenMergerFactory.Create);
            this.bimElementChildrenMerger =
                new Lazy<IChildrenMerger<Objective, BimElement>>(bimElementChildrenMergerFactory.Create);
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
                objective => objective.Status,
                objective => objective.ObjectiveTypeID);

            await MergeLocation(tuple).ConfigureAwait(false);
            await dynamicFieldChildrenMerger.Value.MergeChildren(tuple).ConfigureAwait(false);
            await itemChildrenMerger.Value.MergeChildren(tuple).ConfigureAwait(false);
            await bimElementChildrenMerger.Value.MergeChildren(tuple).ConfigureAwait(false);
        }

        private async ValueTask LoadLocation(Objective objective)
        {
            if (objective.Location != null)
                return;

            objective.Location = await context.Set<Objective>()
               .Where(x => x == objective)
               .Select(x => x.Location)
               .FirstOrDefaultAsync()
               .ConfigureAwait(false);
        }

        private async ValueTask MergeLocation(SynchronizingTuple<Objective> tuple)
        {
            if (tuple.All(x => x.Location == null))
                return;

            await tuple.ForEachAsync(LoadLocation).ConfigureAwait(false);

            var locationTuple = new SynchronizingTuple<Location>(
                null,
                tuple.Synchronized.Location,
                tuple.Local.Location,
                tuple.Remote.Location);

            var action = locationTuple.DetermineAction();


            switch (action)
            {
                case SynchronizingAction.Nothing:
                    break;
                case SynchronizingAction.Merge:
                case SynchronizingAction.AddToLocal:
                case SynchronizingAction.AddToRemote:
                    var item = locationTuple.Remote?.Item;

                    if (item is { Project: null, Objectives: null })
                        item.ProjectID = tuple.Remote.Project.ID;

                    await locationMerger.Merge(locationTuple).ConfigureAwait(false);
                    tuple.ForEachChange(locationTuple, (objective, location) =>
                    {
                        if (objective.Location == location)
                            return false;

                        objective.Location = location;
                        return true;
                    });
                    tuple.SynchronizeChanges(locationTuple);
                    break;
                case SynchronizingAction.RemoveFromLocal:
                case SynchronizingAction.RemoveFromRemote:
                    tuple.ForEachChange(locationTuple, (objective, _) =>
                    {
                        if (objective.Location == null)
                            return false;

                        objective.Location = null;
                        return true;
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
