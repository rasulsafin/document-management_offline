using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Brio.Docs.Updater
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var connectionString = args == null || args.Length == 0
                ? null
                : args[0];
            var startup = new Startup(connectionString);
            var hostBuilder = Host.CreateDefaultBuilder(args)
               .UseSerilog((_, _, configuration) => configuration.ReadFrom.Configuration(startup.Configuration))
               .ConfigureServices(startup.ConfigureServices);
            var host = hostBuilder.Build();
            using var scope = host.Services.CreateScope();
            var migrator = scope.ServiceProvider.GetRequiredService<Migrator>();
            var t = migrator.UpdateDatabase();
            t.Wait();
        }
    }
}
