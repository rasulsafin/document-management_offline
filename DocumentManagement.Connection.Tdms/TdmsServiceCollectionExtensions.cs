using Brio.Docs.Connection.Tdms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TdmsServiceCollectionExtensions
    {
        public static IServiceCollection AddTdms(this IServiceCollection services)
        {
            services.AddSingleton<TdmsConnection>();
            return services;
        }
    }
}
