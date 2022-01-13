using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal class BimElementsMerger : AChildrenMerger<Objective, BimElementObjective, BimElement>
    {
        private readonly BimElementComparer bimElementComparer = new ();

        public BimElementsMerger(
            DMContext context,
            IMerger<BimElement> merger,
            ILogger<BimElementsMerger> logger)
            : base(context, merger, logger)
        {
        }

        protected override Expression<Func<Objective, ICollection<BimElementObjective>>> CollectionExpression
            => objective => objective.BimElements;

        protected override Expression<Func<BimElementObjective, BimElement>> SynchronizableChildExpression
            => link => link.BimElement;

        protected override bool DoesNeedInTuple(BimElement child, SynchronizingTuple<BimElement> childTuple)
            => childTuple.Any(element => bimElementComparer.Equals(element, child));

        private class BimElementComparer
        {
            public bool Equals(BimElement x, BimElement y)
            {
                if (ReferenceEquals(x, null))
                    return false;

                if (ReferenceEquals(y, null))
                    return false;

                if (ReferenceEquals(x, y))
                    return true;

                return string.Equals(x.GlobalID, y.GlobalID, StringComparison.Ordinal) &&
                    string.Equals(x.ParentName, y.ParentName, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
