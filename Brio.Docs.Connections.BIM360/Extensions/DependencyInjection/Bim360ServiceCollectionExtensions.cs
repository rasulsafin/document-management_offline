using Brio.Docs.Connections.Bim360;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Bim360ServiceCollectionExtensions
    {
        public static IServiceCollection AddBim360(this IServiceCollection services)
        {
            services.AddForge();
            services.AddScoped<Bim360Connection>();
            services.AddScoped<EnumerationTypeCreator>();
            services.AddScoped<TypeSubtypeEnumCreator>();
            services.AddScoped<RootCauseEnumCreator>();
            services.AddScoped<LocationEnumCreator>();
            services.AddScoped<AssignToEnumCreator>();
            services.AddScoped<SnapshotFiller>();
            services.AddScoped<Bim360Snapshot>();
            services.AddScoped<Bim360Storage>();
            services.AddScoped<Downloader>();
            services.AddScoped<IfcConfigUtilities>();
            services.AddScoped<IssueSnapshotUtilities>();
            services.AddBim360Synchronization();
            return services;
        }
    }
}
