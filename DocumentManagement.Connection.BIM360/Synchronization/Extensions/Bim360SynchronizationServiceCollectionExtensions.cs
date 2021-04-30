using System;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Factories;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class Bim360SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddBim360Synchronization(this IServiceCollection services)
        {
            services.AddScoped<Func<Bim360ConnectionContext>>(x => x.GetService<Bim360ConnectionContext>);
            services.AddTransient<Bim360ConnectionContext>();
            services.AddScoped<ObjectiveSynchronizerFactory>();
            services.AddScoped<ProjectSynchronizerFactory>();
            services.AddScoped<FoldersSyncHelper>();
            services.AddScoped<HubsHelper>();
            services.AddScoped<ItemsSyncHelper>();
            services.AddScoped<ProjectsHelper>();
            return services;
        }
    }
}
