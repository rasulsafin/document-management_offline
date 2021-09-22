using System;
using Brio.Docs.Connection.LementPro;
using Brio.Docs.Connection.LementPro.Services;
using Brio.Docs.Connection.LementPro.Synchronization;
using Brio.Docs.Connection.LementPro.Synchronization.Factories;
using Brio.Docs.Connection.LementPro.Utilities;
using Brio.Docs.General.Utils.Factories;

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
            services.AddScoped<Func<LementProConnectionContext>>(x => x.GetService<LementProConnectionContext>);
            services.AddScoped<IFactory<LementProConnectionContext>, Factory<LementProConnectionContext>>();

            services.AddScoped<Func<LementProConnectionStorage>>(x => x.GetService<LementProConnectionStorage>);
            services.AddScoped<IFactory<LementProConnectionStorage>, Factory<LementProConnectionStorage>>();

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
