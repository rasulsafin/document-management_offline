using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Strategies;
using Brio.Docs.Synchronization.Utilities.Finders;
using Brio.Docs.Synchronization.Utilities.Mergers;
using Brio.Docs.Synchronization.Utilities.Mergers.ChildrenMergers;
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
            services.AddScoped<ItemStrategy<ObjectiveItemLinker>>();
            services.AddScoped<DynamicFieldStrategy<ObjectiveDynamicFieldLinker>>();

            services.AddScoped<IMerger<Item>, ItemMerger>();
            services.AddScoped<IMerger<DynamicField>, DynamicFieldMerger>();

            services.AddScoped<IAttacher<Item>, ItemAttacher>();

            services.AddScoped<ProjectItemLinker>();
            services.AddScoped<ObjectiveItemLinker>();
            services.AddScoped<ObjectiveDynamicFieldLinker>();

            services.AddObjectiveChildrenMergers();
            services.AddDynamicFieldChildrenMergers();

            return services;
        }

        private static IServiceCollection AddDynamicFieldChildrenMergers(this IServiceCollection services)
            => services.AddSimpleChildrenMerger<DynamicField, DynamicField>(x => x.ChildrenDynamicFields, _ => true);

        private static IServiceCollection AddLinkedChildrenMerger<TParent, TLink, TChild>(
            this IServiceCollection services,
            Expression<Func<TParent, ICollection<TLink>>> getChildrenCollection,
            Expression<Func<TLink, TChild>> getChild,
            Func<TChild, bool> needRemove)
            where TParent : class
            where TLink : class, new()
            where TChild : class, ISynchronizable<TChild>
            => services.AddScoped<IChildrenMerger<TParent, TChild>, ChildrenMerger<TParent, TLink, TChild>>(
                provider => new ChildrenMerger<TParent, TLink, TChild>(
                    provider.GetService<DMContext>(),
                    provider.GetService<IMerger<TChild>>(),
                    getChildrenCollection,
                    getChild,
                    (child, tuple) => tuple.DoesNeed(child),
                    needRemove));

        private static IServiceCollection AddObjectiveChildrenMergers(this IServiceCollection services)
        {
            services.AddLinkedChildrenMerger<Objective, ObjectiveItem, Item>(
                parent => parent.Items,
                link => link.Item,
                item => item.Objectives.Count == 0 && item.Project == null && item.ProjectID == null);
            services.AddSimpleChildrenMerger<DynamicField, DynamicField>(x => x.ChildrenDynamicFields, _ => true);
            services = services.AddScoped<IChildrenMerger<Objective, BimElement>, BimElementsMerger>();
            return services;
        }

        private static IServiceCollection AddSimpleChildrenMerger<TParent, TChild>(
            this IServiceCollection services,
            Expression<Func<TParent, ICollection<TChild>>> getChildrenCollection,
            Func<TChild, bool> needRemove)
            where TParent : class
            where TChild : class, ISynchronizable<TChild>, new()
            => services.AddScoped<IChildrenMerger<TParent, TChild>, SimpleChildrenMerger<TParent, TChild>>(
                provider => new SimpleChildrenMerger<TParent, TChild>(
                    provider.GetService<DMContext>(),
                    provider.GetService<IMerger<TChild>>(),
                    getChildrenCollection,
                    (child, tuple) => tuple.DoesNeed(child),
                    needRemove));
    }
}
