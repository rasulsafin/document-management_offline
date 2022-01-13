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

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal abstract class AChildrenMerger<TParent, TChild, TSynchronizableChild>
        : IChildrenMerger<TParent, TSynchronizableChild>
        where TParent : class
        where TChild : class, new()
        where TSynchronizableChild : class, ISynchronizable<TSynchronizableChild>
    {
        private readonly IAttacher<TSynchronizableChild> attacher;
        private readonly IMerger<TSynchronizableChild> childMerger;
        private readonly DbContext context;
        private readonly Expression<Func<TSynchronizableChild, bool>> defaultNeedToRemoveExpression = child => true;

        protected AChildrenMerger(
            DbContext context,
            IMerger<TSynchronizableChild> childMerger,
            IAttacher<TSynchronizableChild> attacher = null)
        {
            this.context = context;
            this.childMerger = childMerger;
            this.attacher = attacher;

            GetChildrenCollectionFunc =
                new Lazy<Func<TParent, ICollection<TChild>>>(() => CollectionExpression.Compile());
            ChildrenCollectionProperty = new Lazy<PropertyInfo>(() => CollectionExpression.ToPropertyInfo());
            GetChildrenEnumerableExpression = new Lazy<Expression<Func<TParent, IEnumerable<TChild>>>>(
                () =>
                {
                    var parameter = Expression.Parameter(typeof(TParent));
                    return Expression.Lambda<Func<TParent, IEnumerable<TChild>>>(
                        Expression.Property(parameter, ChildrenCollectionProperty.Value),
                        false,
                        parameter);
                });

            IsOneToManyRelationship = new Lazy<bool>(
                () =>
                {
                    var expression = SynchronizableChildExpression.Body;

                    if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
                        expression = unaryExpression.Operand;

                    return expression is ParameterExpression;
                });

            GetSynchronizableChildFunc = new Lazy<Func<TChild, TSynchronizableChild>>(
                () => SynchronizableChildExpression.Compile());
            SynchronizableChildProperty = new Lazy<PropertyInfo>(() => SynchronizableChildExpression.ToPropertyInfo());
        }

        protected abstract Expression<Func<TParent, ICollection<TChild>>> CollectionExpression { get; }

        protected abstract Expression<Func<TChild, TSynchronizableChild>> SynchronizableChildExpression { get; }

        private Lazy<PropertyInfo> ChildrenCollectionProperty { get; }

        private Lazy<Func<TParent, ICollection<TChild>>> GetChildrenCollectionFunc { get; }

        private Lazy<Expression<Func<TParent, IEnumerable<TChild>>>> GetChildrenEnumerableExpression { get; }

        private Lazy<Func<TChild, TSynchronizableChild>> GetSynchronizableChildFunc { get; }

        private Lazy<bool> IsOneToManyRelationship { get; }

        private Lazy<PropertyInfo> SynchronizableChildProperty { get; }

        public async ValueTask MergeChildren(SynchronizingTuple<TParent> tuple)
        {
            if (!await tuple.AnyAsync(HasChildren).ConfigureAwait(false))
                return;

            await tuple.ForEachAsync(LoadChildren).ConfigureAwait(false);

            tuple.ForEach(CreateEmptyChildrenList);

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                GetChildrenCollectionFunc.Value(tuple.Local).Select(GetSynchronizableChildFunc.Value),
                GetChildrenCollectionFunc.Value(tuple.Synchronized).Select(GetSynchronizableChildFunc.Value),
                GetChildrenCollectionFunc.Value(tuple.Remote).Select(GetSynchronizableChildFunc.Value),
                DoesNeedInTuple);

            foreach (var childTuple in tuples)
                await SynchronizeChild(tuple, childTuple).ConfigureAwait(false);
        }

        protected virtual bool DoesNeedInTuple(
            TSynchronizableChild child,
            SynchronizingTuple<TSynchronizableChild> childTuple)
            => childTuple.DoesNeed(child);

        protected virtual Expression<Func<TSynchronizableChild, bool>> GetNeedToRemoveExpression(TParent parent)
            => defaultNeedToRemoveExpression;

        private bool AddChild(TParent parent, TSynchronizableChild child)
        {
            if (!HasChild(parent, child))
            {
                if (!IsOneToManyRelationship.Value)
                {
                    var link = new TChild();
                    SynchronizableChildProperty.Value.SetValue(link, child);
                    GetChildrenCollectionFunc.Value(parent).Add(link);
                    return true;
                }

                GetChildrenCollectionFunc.Value(parent).Add(child as TChild);
                return true;
            }

            return false;
        }

        private void CreateEmptyChildrenList(TParent x)
        {
            if (GetChildrenCollectionFunc.Value(x) == null)
                ChildrenCollectionProperty.Value.SetValue(x, new List<TChild>());
        }

        private bool HasChild(TParent parent, TSynchronizableChild child)
        {
            return GetChildrenCollectionFunc.Value(parent)
               .Any(
                    x =>
                    {
                        var c = GetSynchronizableChildFunc.Value(x);
                        return (c.GetId() != 0 && c.GetId() == child.GetId()) || Equals(c, child);
                    });
        }

        private async ValueTask<bool> HasChildren(TParent parent)
        {
            if (GetChildrenCollectionFunc.Value(parent) == null && parent.GetId() != 0)
            {
                return await context.Set<TParent>()
                   .AsNoTracking()
                   .Where(x => x == parent)
                   .Select(GetChildrenEnumerableExpression.Value)
                   .AnyAsync()
                   .ConfigureAwait(false);
            }

            return (GetChildrenCollectionFunc.Value(parent)?.Count ?? 0) > 0;
        }

        private async ValueTask LoadChildren(TParent parent)
        {
            if (GetChildrenCollectionFunc.Value(parent) == null)
            {
                if (parent.GetId() != 0)
                {
                    var collection = context.Entry(parent)
                       .Collection(GetChildrenEnumerableExpression.Value);

                    if (IsOneToManyRelationship.Value)
                    {
                        await collection.LoadAsync()
                           .ConfigureAwait(false);
                    }
                    else
                    {
                        await collection.Query()
                           .Include(SynchronizableChildExpression)
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
                if (await context.Set<TSynchronizableChild>()
                       .AsNoTracking()
                       .Where(x => x == child)
                       .AnyAsync(GetNeedToRemoveExpression(parent))
                       .ConfigureAwait(false))
                    context.Set<TSynchronizableChild>().Remove(child);
            }

            return result;
        }

        private async ValueTask SynchronizeChild(
            SynchronizingTuple<TParent> tuple,
            SynchronizingTuple<TSynchronizableChild> childTuple)
        {
            var action = childTuple.DetermineAction();

            attacher?.AttachExisting(childTuple);

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
                var first = GetChildrenCollectionFunc.Value(parent)
                   .First(x => GetSynchronizableChildFunc.Value(x) == child);
                GetChildrenCollectionFunc.Value(parent).Remove(first);

                var entry = context.Entry(first);
                if (entry.State == EntityState.Deleted)
                    entry.State = EntityState.Modified;
                return true;
            }

            return false;
        }
    }
}
