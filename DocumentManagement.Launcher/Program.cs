using System;
using CommandLine;

namespace Brio.Docs.Launcher
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var mutName = "Brio.Docs.Launcher";
            using var mutex = new System.Threading.Mutex(true, mutName, out bool createdNew);
            if (!createdNew)
                Environment.Exit(0);

            using var app = new App();
            var options = new AppOptions();

            new Parser(with => with.EnableDashDash = true)
                .ParseArguments<AppOptions>(args)
                .WithParsed(x => options = x);

            App.Options = options;
            app.InitializeComponent();
            app.Run();
        }
    }
}
