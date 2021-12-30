using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal class SimpleChildrenMerger<TParent, TChild> : ChildrenMerger<TParent, TChild, TChild>
        where TParent : class
        where TChild : class, ISynchronizable<TChild>, new()
    {
        public SimpleChildrenMerger(
            DbContext context,
            IMerger<TChild> childMerger,
            Expression<Func<TParent, ICollection<TChild>>> getChildrenCollection,
            Func<TChild, SynchronizingTuple<TChild>, bool> needInTupleFunc,
            Func<TParent, TChild, bool> needRemove)
            : base(context, childMerger, getChildrenCollection, child => child, needInTupleFunc, needRemove)
        {
        }
    }
}
