using MRS.DocumentManagement.Connection.Bim360.Synchronization;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class Bim360SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddBim360Synchronization(this IServiceCollection services)
        {
            services.AddTransient<FoldersSyncHelper>();
            services.AddTransient<HubsHelper>();
            services.AddTransient<ItemsSyncHelper>();
            return services;
        }
    }
}
