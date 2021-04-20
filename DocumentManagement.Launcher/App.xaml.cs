﻿using System;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace DocumentManagement.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            string mutName = "DocumentManagement.Launcher";
            _mutex = new System.Threading.Mutex(true, mutName, out createdNew);
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
