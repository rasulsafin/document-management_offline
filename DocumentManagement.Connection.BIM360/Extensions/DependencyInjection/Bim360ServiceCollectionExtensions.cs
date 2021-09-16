using MRS.DocumentManagement.Connection.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;

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
            services.AddScoped<AssignToEnumCreator>();
            services.AddScoped<SnapshotFiller>();
            services.AddScoped<Bim360Snapshot>();
            services.AddScoped<Bim360Storage>();
            services.AddScoped<Downloader>();
            services.AddScoped<IfcConfigUtilities>();
            services.AddBim360Synchronization();
            return services;
        }
    }
}
