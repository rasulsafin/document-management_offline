using MRS.DocumentManagement.Connection.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Utilities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Bim360ServiceCollectionExtensions
    {
        public static IServiceCollection AddBim360(this IServiceCollection services)
        {
            services.AddForge();
            services.AddScoped<Bim360Connection>();
            services.AddScoped<TypeSubtypeEnumCreator>();
            services.AddScoped<Bim360Storage>();
            services.AddBim360Synchronization();
            return services;
        }
    }
}
