using MRS.DocumentManagement.Synchronization;
using MRS.DocumentManagement.Synchronization.Strategies;
using MRS.DocumentManagement.Synchronization.Utils.Linkers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<Synchronizer>();
            services.AddScoped<ProjectStrategy>();
            services.AddScoped<ItemStrategy<ProjectItemLinker>>();
            services.AddScoped<ObjectiveStrategy>();
            services.AddScoped<ItemStrategy<ObjectiveItemLinker>>();
            services.AddScoped<DynamicFieldStrategy<ObjectiveDynamicFieldLinker>>();
            services.AddScoped<DynamicFieldStrategy<DynamicFieldDynamicFieldLinker>>();
            return services;
        }
    }
}
