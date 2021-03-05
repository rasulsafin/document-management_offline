using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MRS.DocumentManagement.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var mutexName = "Global\\DocumentManagement.Api." + GetAssemblyGuid();

            using (var singleAppMutex = new Mutex(true, mutexName, out bool isNew))
            {
                if (!isNew)
                {
                    Console.WriteLine("DocumentManagement.Api service instance is already running, exiting.");
                    return;
                }

                CreateHostBuilder(args).Build().Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static string GetAssemblyGuid()
        {
            var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
            if (attributes.Length == 0)
                return string.Empty;

            return ((GuidAttribute)attributes[0]).Value;
        }
    }
}
