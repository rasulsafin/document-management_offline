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
using Brio.Docs.Synchronization.Utilities;
using Brio.Docs.Synchronization.Utilities.Finders;
using Brio.Docs.Synchronization.Utilities.Mergers;
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

            services.AddChildrenHelper<DynamicField, DynamicField>(x => x.ChildrenDynamicFields);
            return services;
        }

        private static IServiceCollection AddChildrenHelper<TParent, TChild>(
            this IServiceCollection services,
            Expression<Func<TParent, ICollection<TChild>>> getChildrenCollection)
            where TParent : class
            where TChild : class, ISynchronizable<TChild>
            => services.AddScoped(
                provider => new ChildrenHelper<TParent, TChild>(
                    provider.GetService<DMContext>(),
                    provider.GetService<IMerger<TChild>>(),
                    getChildrenCollection,
                    (child, tuple) => tuple.DoesNeed(child)));
    }
}
