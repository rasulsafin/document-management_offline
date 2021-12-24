using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils;

namespace Brio.Docs.Synchronization.Utilities.Mergers.ChildrenMergers
{
    internal class BimElementsMerger : IChildrenMerger<Objective, BimElement>
    {
        public ValueTask MergeChildren(SynchronizingTuple<Objective> tuple)
        {
            if (!tuple.Any(x => x.BimElements is { Count: > 0 }))
                return ValueTask.CompletedTask;

            tuple.ForEach(x => x.BimElements ??= new List<BimElementObjective>());

            CollectionUtils.Merge(
                tuple,
                tuple.Local.BimElements,
                tuple.Synchronized.BimElements,
                tuple.Remote.BimElements,
                (a, b) => string.Equals(a.ParentName, b.ParentName, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(a.GlobalID, b.GlobalID),
                (element, objective) => new BimElementObjective
                {
                    BimElement = element,
                    Objective = objective,
                },
                x => x.BimElement);
            return ValueTask.CompletedTask;
        }
    }
}
