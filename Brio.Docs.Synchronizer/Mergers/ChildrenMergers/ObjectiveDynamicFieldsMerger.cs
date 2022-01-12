using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Interfaces;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal class ObjectiveDynamicFieldsMerger : ASimpleChildrenMerger<Objective, DynamicField>
    {
        public ObjectiveDynamicFieldsMerger(DMContext context, IMerger<DynamicField> childMerger)
            : base(context, childMerger)
        {
        }

        protected override Expression<Func<Objective, ICollection<DynamicField>>> CollectionExpression
            => objective => objective.DynamicFields;
    }
}
