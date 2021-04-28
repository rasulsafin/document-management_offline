using System;
using MRS.DocumentManagement.Connection.LementPro;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Synchronization;
using MRS.DocumentManagement.Connection.LementPro.Synchronization.Factories;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.General.Utils.Factories;

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
