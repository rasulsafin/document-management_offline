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
    internal class
        ChildrenMerger<TParent, TChild, TSynchronizableChild> : IChildrenMerger<TParent, TSynchronizableChild>
        where TParent : class
        where TChild : class, new()
        where TSynchronizableChild : class, ISynchronizable<TSynchronizableChild>
    {
        private readonly IMerger<TSynchronizableChild> childMerger;
        private readonly PropertyInfo collectionPropertyInfo;
        private readonly DbContext context;
        private readonly Func<TParent, ICollection<TChild>> getCollectionFunc;
        private readonly Expression<Func<TParent, IEnumerable<TChild>>> getEnumerableExpression;
        private readonly Expression<Func<TChild, TSynchronizableChild>> getSynchronizableChildExpression;
        private readonly Func<TChild, TSynchronizableChild> getSynchronizableChildFunc;
        private readonly Func<TSynchronizableChild, SynchronizingTuple<TSynchronizableChild>, bool> needInTupleFunc;
        private readonly PropertyInfo synchronizableChildProperty;
        private readonly Func<TParent, TSynchronizableChild, bool> needRemove;

        public ChildrenMerger(
            DbContext context,
            IMerger<TSynchronizableChild> childMerger,
            Expression<Func<TParent, ICollection<TChild>>> getCollectionExpression,
            Expression<Func<TChild, TSynchronizableChild>> getSynchronizableChild,
            Func<TSynchronizableChild, SynchronizingTuple<TSynchronizableChild>, bool> needInTupleFunc,
            Func<TParent, TSynchronizableChild, bool> needRemove)
        {
            this.context = context;
            this.childMerger = childMerger;
            this.needInTupleFunc = needInTupleFunc;
            getEnumerableExpression = Expression.Lambda<Func<TParent, IEnumerable<TChild>>>(
                getCollectionExpression.Body,
                getCollectionExpression.TailCall,
                getCollectionExpression.Parameters);
            getCollectionFunc = getCollectionExpression.Compile();
            collectionPropertyInfo = getCollectionExpression.ToPropertyInfo();
            this.needRemove = needRemove;

            var expression = getSynchronizableChild.Body;

            if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
                expression = unaryExpression.Operand;

            if (expression is ParameterExpression)
            {
                getSynchronizableChildExpression = null;
                getSynchronizableChildFunc = getSynchronizableChild.Compile();
                synchronizableChildProperty = null;
            }
            else
            {
                getSynchronizableChildExpression = getSynchronizableChild;
                getSynchronizableChildFunc = getSynchronizableChild.Compile();
                synchronizableChildProperty = getSynchronizableChild.ToPropertyInfo();
            }
        }

        public async ValueTask MergeChildren(SynchronizingTuple<TParent> tuple)
        {
            if (!await tuple.AnyAsync(HasChildren).ConfigureAwait(false))
                return;

            await tuple.ForEachAsync(LoadChildren).ConfigureAwait(false);

            tuple.ForEach(CreateEmptyChildrenList);

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                getCollectionFunc(tuple.Local).Select(getSynchronizableChildFunc),
                getCollectionFunc(tuple.Synchronized).Select(getSynchronizableChildFunc),
                getCollectionFunc(tuple.Remote).Select(getSynchronizableChildFunc),
                needInTupleFunc);

            foreach (var childTuple in tuples)
                await SynchronizeChild(tuple, childTuple).ConfigureAwait(false);
        }

        private bool AddChild(TParent parent, TSynchronizableChild child)
        {
            if (!HasChild(parent, child))
            {
                if (synchronizableChildProperty != null)
                {
                    var link = new TChild();
                    synchronizableChildProperty.SetValue(link, child);
                    getCollectionFunc(parent).Add(link);
                    return true;
                }

                getCollectionFunc(parent).Add(child as TChild);
                return true;
            }

            return false;
        }

        private void CreateEmptyChildrenList(TParent x)
        {
            if (getCollectionFunc(x) == null)
                collectionPropertyInfo.SetValue(x, new List<TSynchronizableChild>());
        }

        private bool HasChild(TParent parent, TSynchronizableChild child)
        {
            return getCollectionFunc(parent)
               .Any(
                    x =>
                    {
                        var c = getSynchronizableChildFunc(x);
                        return (c.GetId() != 0 && c.GetId() == child.GetId()) || Equals(c, child);
                    });
        }

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
                    var collection = context.Entry(parent)
                       .Collection(getEnumerableExpression);

                    if (getSynchronizableChildExpression == null)
                    {
                        await collection.LoadAsync()
                           .ConfigureAwait(false);
                    }
                    else
                    {
                        await collection.Query()
                           .Include(getSynchronizableChildExpression)
                           .LoadAsync()
                           .ConfigureAwait(false);
                    }
                }
            }
        }

        private async ValueTask<bool> RemoveChild(
            TParent parent,
            TSynchronizableChild child)
        {
            if (child == null)
                return false;

            var result = UnlinkChild(parent, child);

            if (child.GetId() != 0)
            {
                if (await context.Set<TSynchronizableChild>().AsNoTracking().ContainsAsync(child).ConfigureAwait(false))
                {
                    if (needRemove(parent, child))
                        context.Set<TSynchronizableChild>().Remove(child);
                }
            }

            return result;
        }

        private async ValueTask SynchronizeChild(
            SynchronizingTuple<TParent> tuple,
            SynchronizingTuple<TSynchronizableChild> childTuple)
        {
            var action = childTuple.DetermineAction();

            if (action is SynchronizingAction.Merge
                or SynchronizingAction.AddToLocal
                or SynchronizingAction.AddToRemote)
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
            TSynchronizableChild child)
        {
            if (HasChild(parent, child))
            {
                var first = getCollectionFunc(parent).First(x => getSynchronizableChildFunc(x) == child);
                getCollectionFunc(parent).Remove(first);
                return true;
            }

            return false;
        }
    }
}
