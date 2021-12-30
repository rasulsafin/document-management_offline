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
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Strategies;
using Brio.Docs.Synchronization.Utilities.Finders;
using Brio.Docs.Synchronization.Mergers;
using Brio.Docs.Synchronization.Mergers.ChildrenMergers;
using Brio.Docs.Synchronization.Utils.Linkers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<Synchronizer>();
            services.AddScoped<ISynchronizationStrategy<Project, ProjectExternalDto>, ProjectStrategy>();
            services.AddScoped<ISynchronizationStrategy<Objective, ObjectiveExternalDto>, ObjectiveStrategy>();

            services.AddScoped<ItemStrategy<ProjectItemLinker>>();

            services.AddScoped<IMerger<Objective>, ObjectiveMerger>();
            services.AddScoped<IMerger<Item>, ItemMerger>();
            services.AddScoped<IMerger<DynamicField>, DynamicFieldMerger>();
            services.AddScoped<IMerger<Location>, LocationMerger>();

            services.AddScoped<IAttacher<Item>, ItemAttacher>();

            services.AddScoped<IExternalIdUpdater<DynamicField>, DynamicFieldExternalIdUpdater>();

            services.AddScoped<ProjectItemLinker>();

            services.AddObjectiveChildrenMergers();
            services.AddDynamicFieldChildrenMergers();

            return services;
        }

        private static IServiceCollection AddDynamicFieldChildrenMergers(this IServiceCollection services)
        {
            services.AddSimpleChildrenMerger<DynamicField, DynamicField>(
                x => x.ChildrenDynamicFields,
                (_, _) => true);

            services.AddFactory<IChildrenMerger<DynamicField, DynamicField>>();

            return services;
        }

        private static IServiceCollection AddLinkedChildrenMerger<TParent, TLink, TChild>(
            this IServiceCollection services,
            Expression<Func<TParent, ICollection<TLink>>> getChildrenCollection,
            Expression<Func<TLink, TChild>> getChild,
            Func<TParent, TChild, bool> needRemove,
            Func<TChild, SynchronizingTuple<TChild>, bool> needInTupleFunc = null)
            where TParent : class
            where TLink : class, new()
            where TChild : class, ISynchronizable<TChild>
            => services.AddScoped<IChildrenMerger<TParent, TChild>>(
                provider => new ChildrenMerger<TParent, TLink, TChild>(
                    provider.GetService<DMContext>(),
                    provider.GetService<IMerger<TChild>>(),
                    getChildrenCollection,
                    getChild,
                    needInTupleFunc ?? ((child, tuple) => tuple.DoesNeed(child)),
                    needRemove));

        private static IServiceCollection AddObjectiveChildrenMergers(this IServiceCollection services)
        {
            services.AddLinkedChildrenMerger<Objective, ObjectiveItem, Item>(
                parent => parent.Items,
                link => link.Item,
                (objective, item)
                    => item.Objectives.All(x => x.Objective == objective) &&
                    item.Project == null &&
                    item.ProjectID == null,
                (item, tuple)
                    => tuple.DoesNeed(item) ||
                    item.RelativePath == (string)tuple.GetPropertyValue(nameof(Item.RelativePath)));
            services.AddSimpleChildrenMerger<Objective, DynamicField>(objective => objective.DynamicFields, (_, _) => true);
            services.AddScoped<IChildrenMerger<Objective, BimElement>, BimElementsMerger>();

            services.AddFactory<IChildrenMerger<Objective, Item>>();
            services.AddFactory<IChildrenMerger<Objective, DynamicField>>();
            services.AddFactory<IChildrenMerger<Objective, BimElement>>();
            return services;
        }

        private static IServiceCollection AddSimpleChildrenMerger<TParent, TChild>(
            this IServiceCollection services,
            Expression<Func<TParent, ICollection<TChild>>> getChildrenCollection,
            Func<TParent, TChild, bool> needRemove)
            where TParent : class
            where TChild : class, ISynchronizable<TChild>, new()
            => services.AddScoped<IChildrenMerger<TParent, TChild>>(
                provider => new SimpleChildrenMerger<TParent, TChild>(
                    provider.GetService<DMContext>(),
                    provider.GetService<IMerger<TChild>>(),
                    getChildrenCollection,
                    (child, tuple) => tuple.DoesNeed(child),
                    needRemove));
    }
}
