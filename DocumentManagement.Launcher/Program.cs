using System;
using CommandLine;

namespace MRS.DocumentManagement.Launcher
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var mutName = "MRS.DocumentManagement.Launcher";
            using var mutex = new System.Threading.Mutex(true, mutName, out bool createdNew);
            if (!createdNew)
                Environment.Exit(0);

            using var app = new MRS.DocumentManagement.Launcher.App();
            var options = new AppOptions();

            Parser.Default.ParseArguments<AppOptions>(args)
                .WithParsed(x => options = x);

            App.Options = options;
            app.InitializeComponent();
            app.Run();
        }
    }
}
