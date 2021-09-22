using Brio.Docs.Connection.Bim360.Forge;
using Brio.Docs.Connection.Bim360.Forge.Services;
using Brio.Docs.Connection.Bim360.Forge.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ForgeServiceCollectionExtensions
    {
        public static IServiceCollection AddForge(this IServiceCollection services)
        {
            services.AddScoped<AuthenticationService>();
            services.AddScoped<AccountAdminService>();
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
            services.AddScoped<AppTokenHelper>();
            return services;
        }
    }
}
