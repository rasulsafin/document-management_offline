using Brio.Docs.Connections.Bim360.Forge;
using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ForgeServiceCollectionExtensions
    {
        public static IServiceCollection AddForge(this IServiceCollection services)
        {
            services.AddScoped<AuthenticationService>();
            services.AddScoped<AccountAdminService>();
            services.AddScoped<IUsersGetter>(x => x.GetService<AccountAdminService>());
            services.AddScoped<FoldersService>();
            services.AddScoped<HubsService>();
            services.AddScoped<IIssuesService, IssuesService>();
            services.AddScoped<LocationService>();
            services.AddScoped<ItemsService>();
            services.AddScoped<ObjectsService>();
            services.AddScoped<ProjectsService>();
            services.AddScoped<VersionsService>();

            services.AddScoped<Authenticator>();
            services.AddScoped<ForgeConnection>();
            services.AddScoped<TokenHelper>();
            services.AddScoped<AppTokenHelper>();
            services.AddScoped<IssueUpdatesFinder>();
            return services;
        }
    }
}