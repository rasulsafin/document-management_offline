using System;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization;
using MRS.DocumentManagement.Synchronization.Interfaces;
using MRS.DocumentManagement.Synchronization.Strategies;
using MRS.DocumentManagement.Synchronization.Utils.Linkers;

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
