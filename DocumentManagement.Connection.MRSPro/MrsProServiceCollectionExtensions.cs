using MRS.DocumentManagement.Connection.MrsPro;
using MRS.DocumentManagement.Connection.MrsPro.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MrsProServiceCollectionExtensions
    {
        public static IServiceCollection AddMrsPro(this IServiceCollection services)
        {
            services.AddScoped<MrsProConnection>();
            services.AddScoped<MrsProHttpConnection>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<ProjectService>();
            return services;
        }
    }
}
