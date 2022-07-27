using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Brio.Docs.Updater
{
    public class Startup
    {
        private readonly string connectionString;

        public Startup(string connectionString)
        {
            this.connectionString = connectionString;
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");

#if DEBUG
            builder.AddJsonFile("appsettings.Debug.json");
#endif

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        private string ConnectionString
            => string.IsNullOrEmpty(connectionString)
                ? Configuration.GetConnectionString("DefaultConnection")
                : connectionString;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Database.DMContext>(options => options.UseSqlite(ConnectionString));
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            services.AddTransient<Migrator>();
        }
    }
}
