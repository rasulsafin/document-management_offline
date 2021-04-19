using MRS.DocumentManagement.Connection.Tdms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TdmsServiceCollectionExtensions
    {
        public static IServiceCollection AddTdms(this IServiceCollection services)
        {
            services.AddScoped<TdmsConnection>();
            return services;
        }
    }
}
