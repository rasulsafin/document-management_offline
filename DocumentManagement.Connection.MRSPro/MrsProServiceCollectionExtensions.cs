using System;
using MRS.DocumentManagement.Connection.MrsPro;
using MRS.DocumentManagement.Connection.MrsPro.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MrsProServiceCollectionExtensions
    {
        public static IServiceCollection AddMrsPro(this IServiceCollection services)
        {
            services.AddScoped<MrsProHttpConnection>();
            services.AddScoped<MrsProConnection>();
            services.AddScoped<MrsProConnectionContext>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<ProjectService>();
            services.AddScoped<ObjectiveService>();

            services.AddScoped<Func<MrsProConnectionContext>>(x => x.GetService<MrsProConnectionContext>);

            return services;
        }
    }
}
