using Brio.Docs.Connections.Bim360;
using Brio.Docs.Connections.Bim360.Interfaces;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Bim360ServiceCollectionExtensions
    {
        public static IServiceCollection AddBim360(this IServiceCollection services)
        {
            services.AddForge();
            services.AddScoped<Bim360Connection>();
            services.AddScoped<EnumerationTypeCreator>();

            services
               .AddEnumCreator<TypeSubtypeEnumCreator, IssueTypeSnapshot>()
               .AddEnumCreator<RootCauseEnumCreator, RootCauseSnapshot>()
               .AddEnumCreator<LocationEnumCreator, LocationSnapshot>()
               .AddEnumCreator<AssignToEnumCreator, AssignToVariant>()
               .AddEnumCreator<StatusEnumCreator, StatusSnapshot>();

            services.AddSnapshotUtilities();
            services.AddScoped<Bim360Storage>();
            services.AddScoped<Downloader>();
            services.AddScoped<ConfigurationsHelper>();
            services.AddBim360Synchronization();
            return services;
        }

        private static IServiceCollection AddEnumCreator<TCreator, TSnapshot>(this IServiceCollection services)
            where TCreator : class, IEnumIdentification<TSnapshot>
            => services
               .AddScoped<TCreator>()
               .AddScoped<IEnumIdentification<TSnapshot>, TCreator>(x => x.GetService<TCreator>());

        private static IServiceCollection AddSnapshotUtilities(this IServiceCollection services)
        {
            services.AddScoped<Bim360Snapshot>();
            services.AddScoped<SnapshotFiller>();
            services.AddScoped<SnapshotGetter>();
            services.AddScoped<SnapshotUpdater>();

            services.AddScoped<ProjectSnapshotUtilities>();
            services.AddScoped<IssueSnapshotUtilities>();
            return services;
        }
    }
}