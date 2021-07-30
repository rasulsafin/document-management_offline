using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ForgeServiceCollectionExtensions
    {
        public static IServiceCollection AddForge(this IServiceCollection services)
        {
            services.AddScoped<AuthenticationService>();
            services.AddScoped<FoldersService>();
            services.AddScoped<HubsService>();
            services.AddScoped<IssuesService>();
            services.AddScoped<ItemsService>();
            services.AddScoped<ObjectsService>();
            services.AddScoped<ProjectsService>();
            services.AddScoped<VersionsService>();

            services.AddScoped<Authenticator>();
            services.AddScoped<ForgeConnection>();
            services.AddScoped<TokenHelper>();
            return services;
        }
    }
}