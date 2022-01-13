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
using Microsoft.Extensions.Logging;

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

        private readonly Lazy<PropertyInfo> lazyChildrenCollectionProperty;
        private readonly Lazy<Func<TParent, ICollection<TChild>>> lazyGetChildrenCollectionFunc;
        private readonly Lazy<Expression<Func<TParent, IEnumerable<TChild>>>> lazyGetChildrenEnumerableExpression;
        private readonly Lazy<Func<TChild, TSynchronizableChild>> lazyGetSynchronizableChildFunc;
        private readonly Lazy<bool> lazyIsOneToManyRelationship;
        private readonly Lazy<PropertyInfo> lazySynchronizableChildProperty;

        private readonly ILogger<AChildrenMerger<TParent, TChild, TSynchronizableChild>> logger;

        protected AChildrenMerger(
            DbContext context,
            IMerger<TSynchronizableChild> childMerger,
            ILogger<AChildrenMerger<TParent, TChild, TSynchronizableChild>> logger,
            IAttacher<TSynchronizableChild> attacher = null)
        {
            this.context = context;
            this.childMerger = childMerger;
            this.logger = logger;
            this.attacher = attacher;

            lazyGetChildrenCollectionFunc =
                new Lazy<Func<TParent, ICollection<TChild>>>(() => CollectionExpression.Compile());
            lazyChildrenCollectionProperty = new Lazy<PropertyInfo>(() => CollectionExpression.ToPropertyInfo());
            lazyGetChildrenEnumerableExpression = new Lazy<Expression<Func<TParent, IEnumerable<TChild>>>>(
                () =>
                {
                    var parameter = Expression.Parameter(typeof(TParent));
                    return Expression.Lambda<Func<TParent, IEnumerable<TChild>>>(
                        Expression.Property(parameter, ChildrenCollectionProperty),
                        false,
                        parameter);
                });

            lazyIsOneToManyRelationship = new Lazy<bool>(
                () =>
                {
                    var expression = SynchronizableChildExpression.Body;

                    if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
                        expression = unaryExpression.Operand;

                    return expression is ParameterExpression;
                });

            lazyGetSynchronizableChildFunc = new Lazy<Func<TChild, TSynchronizableChild>>(
                () => SynchronizableChildExpression.Compile());
            lazySynchronizableChildProperty = new Lazy<PropertyInfo>(() => SynchronizableChildExpression.ToPropertyInfo());

            logger.LogTrace("Base initialization of children merger completed");
        }

        protected abstract Expression<Func<TParent, ICollection<TChild>>> CollectionExpression { get; }

        protected abstract Expression<Func<TChild, TSynchronizableChild>> SynchronizableChildExpression { get; }

        private PropertyInfo ChildrenCollectionProperty => lazyChildrenCollectionProperty.Value;

        private Func<TParent, ICollection<TChild>> GetChildrenCollectionFunc => lazyGetChildrenCollectionFunc.Value;

        private Expression<Func<TParent, IEnumerable<TChild>>> GetChildrenEnumerableExpression => lazyGetChildrenEnumerableExpression.Value;

        private Func<TChild, TSynchronizableChild> GetSynchronizableChildFunc => lazyGetSynchronizableChildFunc.Value;

        private bool IsOneToManyRelationship => lazyIsOneToManyRelationship.Value;

        private PropertyInfo SynchronizableChildProperty => lazySynchronizableChildProperty.Value;

        public async ValueTask MergeChildren(SynchronizingTuple<TParent> tuple)
        {
            logger.LogTrace("MergeChildren started for tuple {Id}", tuple.ExternalID);
            if (!await tuple.AnyAsync(HasChildren).ConfigureAwait(false))
                return;

            logger.LogDebug("Tuple has children");
            await tuple.ForEachAsync(LoadChildren).ConfigureAwait(false);
            tuple.ForEach(CreateEmptyChildrenList);
            logger.LogTrace("Children loaded");

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                GetChildrenCollectionFunc(tuple.Local).Select(GetSynchronizableChildFunc),
                GetChildrenCollectionFunc(tuple.Synchronized).Select(GetSynchronizableChildFunc),
                GetChildrenCollectionFunc(tuple.Remote).Select(GetSynchronizableChildFunc),
                DoesNeedInTuple);

            logger.LogDebug("Created {Count} tuples", tuples.Count);
            foreach (var childTuple in tuples)
                await SynchronizeChild(tuple, childTuple).ConfigureAwait(false);
            logger.LogTrace("Children synchronized");
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
                if (!IsOneToManyRelationship)
                {
                    var link = new TChild();
                    SynchronizableChildProperty.SetValue(link, child);
                    GetChildrenCollectionFunc(parent).Add(link);
                    return true;
                }

                GetChildrenCollectionFunc(parent).Add(child as TChild);
                return true;
            }

            return false;
        }

        private void CreateEmptyChildrenList(TParent x)
        {
            if (GetChildrenCollectionFunc(x) == null)
                ChildrenCollectionProperty.SetValue(x, new List<TChild>());
        }

        private bool HasChild(TParent parent, TSynchronizableChild child)
        {
            return GetChildrenCollectionFunc(parent)
               .Any(
                    x =>
                    {
                        var c = GetSynchronizableChildFunc(x);
                        return (c.GetId() != 0 && c.GetId() == child.GetId()) || Equals(c, child);
                    });
        }

        private async ValueTask<bool> HasChildren(TParent parent)
        {
            if (GetChildrenCollectionFunc(parent) == null && parent.GetId() != 0)
            {
                return await context.Set<TParent>()
                   .AsNoTracking()
                   .Where(x => x == parent)
                   .Select(GetChildrenEnumerableExpression)
                   .AnyAsync()
                   .ConfigureAwait(false);
            }

            return (GetChildrenCollectionFunc(parent)?.Count ?? 0) > 0;
        }

        private async ValueTask LoadChildren(TParent parent)
        {
            if (GetChildrenCollectionFunc(parent) == null)
            {
                if (parent.GetId() != 0)
                {
                    var collection = context.Entry(parent)
                       .Collection(GetChildrenEnumerableExpression);

                    if (IsOneToManyRelationship)
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
                var first = GetChildrenCollectionFunc(parent)
                   .First(x => GetSynchronizableChildFunc(x) == child);
                GetChildrenCollectionFunc(parent).Remove(first);

                var entry = context.Entry(first);
                if (entry.State == EntityState.Deleted)
                    entry.State = EntityState.Modified;
                return true;
            }

            return false;
        }
    }
}
