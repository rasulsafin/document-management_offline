using System;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace MRS.DocumentManagement.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private const string DEVELOP = "--develop";
        private const string LOCALIZATE = "language=";
        private TaskbarIcon notifyIcon;
        private Mutex mutex;

        public static bool Develop { get; private set; } = false;

        public void Dispose() => mutex?.Dispose();

        protected override void OnStartup(StartupEventArgs e)
        {
            foreach (var arg in e.Args)
            {
                if (arg.ToLower() == DEVELOP)
                {
                    Develop = true;
                }
                else if (arg.ToLower().StartsWith(LOCALIZATE))
                {
                    var culture = arg.Replace(LOCALIZATE, string.Empty);
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
                }
            }

            bool createdNew;
            string mutName = "MRS.DocumentManagement.Launcher";
            mutex = new System.Threading.Mutex(true, mutName, out createdNew);
            if (!createdNew)
            {
                Shutdown();
                return;
            }

            base.OnStartup(e);

            // create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.Dispose();
            if (notifyIcon != null)
            {
                notifyIcon.Dispose(); // the icon would clean up automatically, but this is cleaner
                if (notifyIcon.DataContext is IDisposable disposable)
                    disposable.Dispose();
            }

            base.OnExit(e);
        }
    }
}
