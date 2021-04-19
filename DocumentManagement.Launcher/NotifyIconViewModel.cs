using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using DocumentManagement.Launcher.Base;
using DocumentManagement.Launcher.Dialogs;

namespace DocumentManagement.Launcher
{
    public class NotifyIconViewModel : BaseViewModel, IDisposable
    {
        #region field
        private bool isRunDm;
        private Process dmProcess;
        #endregion

        #region constructor
        public NotifyIconViewModel()
        {
            VisibleConsoleCommand = new HCommand(VisibleConsole);
            ExitApplicationCommand = new HCommand(ExitApplication);
            OpenSwaggerCommand = new HCommand(OpenSwagger);
            StartDocumentMenagermentProcess();
        }
        #endregion

        #region binding
        public bool IsRunDm
        {
            get => isRunDm;
            set
            {
                isRunDm = value;
                OnPropertyChanged();
            }
        }

        public HCommand ExitApplicationCommand { get; }

        public HCommand VisibleConsoleCommand { get; }

        public HCommand OpenSwaggerCommand { get; }

        public bool IsVisibleConsole
        {
            get => LauncherSettings.Instance.VisibleConsole;
            set
            {
                LauncherSettings.Instance.VisibleConsole = value;
                LauncherSettings.Instance.Save();
            }
        }
        #endregion

        public void Dispose()
        {
            dmProcess?.Kill();
            dmProcess?.Dispose();
        }

        #region private method
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void OpenSwagger() => OpenUrl(LauncherSettings.Instance.SwaggerPath);

        private void DmProcessDisposed(object sender, EventArgs e) => IsRunDm = false;

        private void StartDocumentMenagermentProcess()
        {
            dmProcess = Process.GetProcessesByName("DocumentManagement.Api").FirstOrDefault();
            if (dmProcess != null)
            {
                dmProcess.Kill();
            }

            string path = LauncherSettings.Instance.DMApiPath;
            if (!File.Exists(path))
            {
                WinBox.ShowMessage($"Файл не найден!\n{path}");
                return;
            }

            dmProcess = new Process();
            dmProcess.StartInfo.FileName = path;
            dmProcess.StartInfo.CreateNoWindow = !IsVisibleConsole;
            IsRunDm = true;
            dmProcess.Start();
            dmProcess.EnableRaisingEvents = true;
            dmProcess.Exited += DmProcessDisposed;
            dmProcess.Disposed += DmProcessDisposed;
        }

        private void VisibleConsole()
        {
            if (dmProcess == null)
            {
                StartDocumentMenagermentProcess();
            }
            else
            {
                ShowWindow(dmProcess.MainWindowHandle, IsVisibleConsole ? 0 : 5);
                IsVisibleConsole = !IsVisibleConsole;
            }
        }

        private void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion
    }
}
