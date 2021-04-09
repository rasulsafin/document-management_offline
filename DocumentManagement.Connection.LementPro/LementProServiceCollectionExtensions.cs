using MRS.DocumentManagement.Connection.LementPro;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LementProServiceCollectionExtensions
    {
        public static IServiceCollection AddYandexDisk(this IServiceCollection services)
        {
            services.AddScoped<LementProConnection>();
            return services;
        }
    }
}
