using MRS.DocumentManagement.Connection.LementPro;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Synchronization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LementProServiceCollectionExtensions
    {
        public static IServiceCollection AddLementPro(this IServiceCollection services)
        {
            services.AddScoped<LementProConnection>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<BimsService>();
            services.AddScoped<ProjectsService>();
            services.AddScoped<TasksService>();
            services.AddTransient<LementProConnectionContext>();
            return services;
        }
    }
}
