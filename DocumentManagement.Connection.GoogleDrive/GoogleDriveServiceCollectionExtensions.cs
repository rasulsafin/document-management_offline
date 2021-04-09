using MRS.DocumentManagement.Connection.GoogleDrive;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GoogleDriveServiceCollectionExtensions
    {
        public static IServiceCollection AddYandexDisk(this IServiceCollection services)
        {
            services.AddScoped<GoogleConnection>();
            return services;
        }
    }
}
