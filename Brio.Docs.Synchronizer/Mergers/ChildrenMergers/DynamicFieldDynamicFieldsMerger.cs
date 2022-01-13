using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Interfaces;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal class DynamicFieldDynamicFieldsMerger : ASimpleChildrenMerger<DynamicField, DynamicField>
    {
        private readonly Expression<Func<DynamicField, ICollection<DynamicField>>> collectionExpression =
            field => field.ChildrenDynamicFields;

        public DynamicFieldDynamicFieldsMerger(DMContext context, IMerger<DynamicField> childMerger)
            : base(context, childMerger)
        {
        }

        protected override Expression<Func<DynamicField, ICollection<DynamicField>>> CollectionExpression
            => collectionExpression;
    }
}
