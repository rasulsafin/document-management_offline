using System;
using Brio.Docs.Connections.LementPro;
using Brio.Docs.Connections.LementPro.Services;
using Brio.Docs.Connections.LementPro.Synchronization;
using Brio.Docs.Connections.LementPro.Synchronization.Factories;
using Brio.Docs.Connections.LementPro.Utilities;
using Brio.Docs.Integration.Factories;

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
            services.AddScoped<ProjectsService>();
            services.AddScoped<TasksService>();

            services.AddScoped<CommonRequestsUtility>();
            services.AddScoped<HttpRequestUtility>();

            services.AddTransient<LementProConnectionContext>();
            services.AddFactory<LementProConnectionContext>();
            services.AddScoped<LementProConnectionStorage>();
            services.AddFactory<LementProConnectionStorage>();

            services.AddScoped<
                IFactory<LementProConnectionContext, LementProObjectivesSynchronizer>,
                ObjectiveSynchronizerFactory>();
            services.AddScoped<
                IFactory<LementProConnectionContext, LementProProjectsSynchronizer>,
                ProjectSynchronizerFactory>();

            return services;
        }
    }
}
