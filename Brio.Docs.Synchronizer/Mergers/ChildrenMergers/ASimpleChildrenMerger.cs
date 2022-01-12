using System;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal abstract class ASimpleChildrenMerger<TParent, TChild> : AChildrenMerger<TParent, TChild, TChild>
        where TParent : class
        where TChild : class, ISynchronizable<TChild>, new()
    {
        protected ASimpleChildrenMerger(DbContext context, IMerger<TChild> childMerger, IAttacher<TChild> attacher = null)
            : base(context, childMerger, attacher)
        {
        }

        protected override Expression<Func<TChild, TChild>> SynchronizableChildExpression => child => child;
    }
}
