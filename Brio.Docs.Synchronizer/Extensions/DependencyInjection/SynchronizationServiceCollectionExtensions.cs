using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Mergers;
using Brio.Docs.Synchronization.Mergers.ChildrenMergers;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Strategies;
using Brio.Docs.Synchronization.Utilities.Finders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<Synchronizer>();
            services.AddScoped<ISynchronizationStrategy<Project, ProjectExternalDto>, ProjectStrategy>();
            services.AddScoped<ISynchronizationStrategy<Objective, ObjectiveExternalDto>, ObjectiveStrategy>();

            services.AddScoped<IMerger<Project>, ProjectMerger>();
            services.AddScoped<IMerger<Objective>, ObjectiveMerger>();
            services.AddScoped<IMerger<Item>, ItemMerger>();
            services.AddScoped<IMerger<DynamicField>, DynamicFieldMerger>();
            services.AddScoped<IMerger<Location>, LocationMerger>();

            services.AddScoped<IAttacher<Item>, ItemAttacher>();

            services.AddScoped<IExternalIdUpdater<Item>, ItemExternalIdUpdater>();
            services.AddScoped<IExternalIdUpdater<DynamicField>, DynamicFieldExternalIdUpdater>();

            services.AddProjectChildrenMergers();
            services.AddObjectiveChildrenMergers();
            services.AddDynamicFieldChildrenMergers();

            return services;
        }

        private static IServiceCollection AddDynamicFieldChildrenMergers(this IServiceCollection services)
        {
            services.AddSimpleChildrenMerger<DynamicField, DynamicField>(
                x => x.ChildrenDynamicFields,
                _ => child => true);

            services.AddFactory<IChildrenMerger<DynamicField, DynamicField>>();

            return services;
        }

        private static IServiceCollection AddLinkedChildrenMerger<TParent, TLink, TChild>(
            this IServiceCollection services,
            Expression<Func<TParent, ICollection<TLink>>> getChildrenCollection,
            Expression<Func<TLink, TChild>> getChild,
            Func<TParent, Expression<Func<TChild, bool>>> needRemove,
            Func<TChild, SynchronizingTuple<TChild>, bool> needInTupleFunc = null,
            bool needAttacher = false)
            where TParent : class
            where TLink : class, new()
            where TChild : class, ISynchronizable<TChild>
            => services.AddScoped<IChildrenMerger<TParent, TChild>>(
                provider =>
                {
                    var context = provider.GetService<DMContext>();
                    var merger = provider.GetService<IMerger<TChild>>();
                    needInTupleFunc ??= (child, tuple) => tuple.DoesNeed(child);

                    if (needAttacher)
                    {
                        var attacher = provider.GetService<IAttacher<TChild>>();

                        return new ChildrenMerger<TParent, TLink, TChild>(
                            context,
                            merger,
                            getChildrenCollection,
                            getChild,
                            needInTupleFunc,
                            needRemove,
                            attacher);
                    }

                    return new ChildrenMerger<TParent, TLink, TChild>(
                        context,
                        merger,
                        getChildrenCollection,
                        getChild,
                        needInTupleFunc,
                        needRemove);
                });

        private static IServiceCollection AddObjectiveChildrenMergers(this IServiceCollection services)
        {
            services.AddLinkedChildrenMerger<Objective, ObjectiveItem, Item>(
                parent => parent.Items,
                link => link.Item,
                objective => item => item.Objectives.All(x => x.Objective == objective) &&
                    item.Project == null &&
                    item.ProjectID == null,
                DoesNeedItem,
                true);

            services.AddSimpleChildrenMerger<Objective, DynamicField>(
                objective => objective.DynamicFields,
                _ => field => true);
            services.AddScoped<IChildrenMerger<Objective, BimElement>, BimElementsMerger>();

            services.AddFactory<IChildrenMerger<Objective, Item>>();
            services.AddFactory<IChildrenMerger<Objective, DynamicField>>();
            services.AddFactory<IChildrenMerger<Objective, BimElement>>();
            return services;
        }

        private static IServiceCollection AddProjectChildrenMergers(this IServiceCollection services)
        {
            services.AddSimpleChildrenMerger<Project, Item>(
                project => project.Items,
                _ => item => !item.Objectives.Any(),
                DoesNeedItem,
                true);

            services.AddFactory<IChildrenMerger<Project, Item>>();
            return services;
        }

        private static IServiceCollection AddSimpleChildrenMerger<TParent, TChild>(
            this IServiceCollection services,
            Expression<Func<TParent, ICollection<TChild>>> getChildrenCollection,
            Func<TParent, Expression<Func<TChild, bool>>> needRemove,
            Func<TChild, SynchronizingTuple<TChild>, bool> needInTupleFunc = null,
            bool needAttacher = false)
            where TParent : class
            where TChild : class, ISynchronizable<TChild>, new()
            => services.AddScoped<IChildrenMerger<TParent, TChild>>(
                provider =>
                {
                    var dmContext = provider.GetService<DMContext>();
                    var merger = provider.GetService<IMerger<TChild>>();
                    needInTupleFunc ??= (child, tuple) => tuple.DoesNeed(child);

                    if (needAttacher)
                    {
                        var attacher = provider.GetService<IAttacher<TChild>>();

                        return new SimpleChildrenMerger<TParent, TChild>(
                            dmContext,
                            merger,
                            getChildrenCollection,
                            needInTupleFunc,
                            needRemove,
                            attacher);
                    }

                    return new SimpleChildrenMerger<TParent, TChild>(
                        dmContext,
                        merger,
                        getChildrenCollection,
                        needInTupleFunc,
                        needRemove);
                });

        private static bool DoesNeedItem(Item item, SynchronizingTuple<Item> tuple)
        {
            return tuple.DoesNeed(item) ||
                item.RelativePath == (string)tuple.GetPropertyValue(nameof(Item.RelativePath));
        }
    }
}
