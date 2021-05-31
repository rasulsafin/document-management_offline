using System;
using MRS.DocumentManagement.General.Utils.Factories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddScopedFactory<TResult>(this IServiceCollection services)
        {
            services.AddScoped<Func<IServiceScope, TResult>>(
                x => scope => (scope?.ServiceProvider ?? x).GetRequiredService<TResult>());
            services.AddScoped<IFactory<IServiceScope, TResult>, Factory<IServiceScope, TResult>>();
            return services;
        }
    }
}
