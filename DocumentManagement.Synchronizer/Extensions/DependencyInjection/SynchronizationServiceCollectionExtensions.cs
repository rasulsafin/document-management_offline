using System;
using Brio.Docs.Database.Models;
using Brio.Docs.Interface.Dtos;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Strategies;
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
            services.AddScoped<DynamicFieldStrategy<DynamicFieldDynamicFieldLinker>>();

            services.AddScoped<ProjectItemLinker>();
            services.AddScoped<ObjectiveItemLinker>();
            services.AddScoped<ObjectiveDynamicFieldLinker>();
            services.AddScoped<DynamicFieldDynamicFieldLinker>();

            // TODO: Replace with factory.
            services.AddScoped<Func<DynamicFieldStrategy<DynamicFieldDynamicFieldLinker>>>(
                x => x.GetService<DynamicFieldStrategy<DynamicFieldDynamicFieldLinker>>);
            return services;
        }
    }
}
