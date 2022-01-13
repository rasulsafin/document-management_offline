using System;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal abstract class ASimpleChildrenMerger<TParent, TChild> : AChildrenMerger<TParent, TChild, TChild>
        where TParent : class
        where TChild : class, ISynchronizable<TChild>, new()
    {
        private readonly Expression<Func<TChild, TChild>> synchronizableChildExpression = child => child;

        protected ASimpleChildrenMerger(
            DbContext context,
            IMerger<TChild> childMerger,
            ILogger<ASimpleChildrenMerger<TParent, TChild>> logger,
            IAttacher<TChild> attacher = null)
            : base(context, childMerger, logger, attacher)
        {
        }

        protected override Expression<Func<TChild, TChild>> SynchronizableChildExpression => synchronizableChildExpression;
    }
}
