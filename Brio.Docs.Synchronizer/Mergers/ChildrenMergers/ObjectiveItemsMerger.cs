using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal class ObjectiveItemsMerger : AChildrenMerger<Objective, ObjectiveItem, Item>
    {
        public ObjectiveItemsMerger(DMContext context, IMerger<Item> childMerger, IAttacher<Item> attacher)
            : base(context, childMerger, attacher)
        {
        }

        protected override Expression<Func<Objective, ICollection<ObjectiveItem>>> CollectionExpression
            => objective => objective.Items;

        protected override Expression<Func<ObjectiveItem, Item>> SynchronizableChildExpression => link => link.Item;

        protected override bool DoesNeedInTuple(Item child, SynchronizingTuple<Item> childTuple)
            => childTuple.DoesNeed(child) ||
                child.RelativePath == (string)childTuple.GetPropertyValue(nameof(Item.RelativePath));

        protected override Expression<Func<Item, bool>> GetNeedToRemoveExpression(Objective parent)
            => item => item.Objectives.All(x => x.Objective == parent) &&
                item.Project == null &&
                item.ProjectID == null;
    }
}
