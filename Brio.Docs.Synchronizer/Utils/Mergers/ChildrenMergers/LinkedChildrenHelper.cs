using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Brio.Docs.Common.Extensions;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Utilities.Mergers.ChildrenMergers
{
    internal class LinkedChildrenHelper<TParent, TChild> : IChildrenMerger<TParent, TChild>
        where TParent : class
        where TChild : class, ISynchronizable<TChild>
    {
        private readonly IMerger<TChild> childMerger;
        private readonly DbContext context;
        private readonly Func<TParent, ICollection<TChild>> getCollectionFunc;
        private readonly Expression<Func<TParent, IEnumerable<TChild>>> getEnumerableExpression;
        private readonly Func<TChild, SynchronizingTuple<TChild>, bool> needInTupleFunc;
        private readonly PropertyInfo propertyInfo;

        public LinkedChildrenHelper(
            DbContext context,
            IMerger<TChild> childMerger,
            Expression<Func<TParent, ICollection<TChild>>> getChildrenCollection,
            Func<TChild, SynchronizingTuple<TChild>, bool> needInTupleFunc)
        {
            this.context = context;
            this.childMerger = childMerger;
            this.needInTupleFunc = needInTupleFunc;
            getEnumerableExpression = Expression.Lambda<Func<TParent, IEnumerable<TChild>>>(
                getChildrenCollection.Body,
                getChildrenCollection.TailCall,
                getChildrenCollection.Parameters);
            getCollectionFunc = getChildrenCollection.Compile();
            propertyInfo = getChildrenCollection.ToPropertyInfo();
        }

        public async ValueTask MergeChildren(SynchronizingTuple<TParent> tuple)
        {
            if (!await tuple.AnyAsync(HasChildren).ConfigureAwait(false))
                return;

            await tuple.ForEachAsync(LoadChildren).ConfigureAwait(false);

            tuple.ForEach(CreateEmptyChildrenList);

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                getCollectionFunc(tuple.Local),
                getCollectionFunc(tuple.Synchronized),
                getCollectionFunc(tuple.Remote),
                needInTupleFunc);

            foreach (var childTuple in tuples)
                await SynchronizeChild(tuple, childTuple).ConfigureAwait(false);
        }

        private bool AddChild(TParent parent, TChild child)
        {
            if (!HasChild(parent, child))
            {
                getCollectionFunc(parent).Add(child);
                return true;
            }

            return false;
        }

        private void CreateEmptyChildrenList(TParent x)
        {
            if (getCollectionFunc(x) == null)
                propertyInfo.SetValue(x, new List<TChild>());
        }

        private bool HasChild(TParent parent, TChild child)
            => getCollectionFunc(parent).Any(x => (x.GetId() != 0 && x.GetId() == child.GetId()) || x == child);

        private async ValueTask<bool> HasChildren(TParent parent)
        {
            if (getCollectionFunc(parent) == null && parent.GetId() != 0)
            {
                return await context.Entry(parent)
                   .Collection(getEnumerableExpression)
                   .Query()
                   .AnyAsync()
                   .ConfigureAwait(false);
            }

            return (getCollectionFunc(parent)?.Count ?? 0) > 0;
        }

        private async ValueTask LoadChildren(TParent parent)
        {
            if (getCollectionFunc(parent) == null)
            {
                if (parent.GetId() != 0)
                {
                    await context.Entry(parent)
                       .Collection(getEnumerableExpression)
                       .LoadAsync()
                       .ConfigureAwait(false);
                }
            }
        }

        private async ValueTask<bool> RemoveChild(
            TParent parent,
            TChild child)
        {
            var result = UnlinkChild(parent, child);

            if (child.GetId() != 0 && await context.Set<TChild>().ContainsAsync(child).ConfigureAwait(false))
                context.Set<TChild>().Remove(child);

            return result;
        }

        private async ValueTask SynchronizeChild(SynchronizingTuple<TParent> tuple, SynchronizingTuple<TChild> childTuple)
        {
            var action = childTuple.DetermineAction();
            await childMerger.Merge(childTuple).ConfigureAwait(false);

            switch (action)
            {
                case SynchronizingAction.Nothing:
                case SynchronizingAction.Merge:
                    break;
                case SynchronizingAction.AddToLocal:
                case SynchronizingAction.AddToRemote:
                    tuple.ForEachChange(childTuple, AddChild);
                    break;
                case SynchronizingAction.RemoveFromLocal:
                case SynchronizingAction.RemoveFromRemote:
                    await tuple.ForEachChangeAsync(childTuple, RemoveChild).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), "Incorrect action");
            }

            tuple.SynchronizeChanges(childTuple);
        }

        private bool UnlinkChild(
            TParent parent,
            TChild child)
        {
            if (HasChild(parent, child))
            {
                getCollectionFunc(parent).Remove(child);
                return true;
            }

            return false;
        }
    }
}
