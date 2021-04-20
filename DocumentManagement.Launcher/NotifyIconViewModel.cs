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
            StartDmConsoleCommand = new HCommand(StartDocumentMenagermentProcess);
            OpenSettingsCommand = new HCommand(OpenSettings);
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

        public HCommand StartDmConsoleCommand { get; }

        public HCommand OpenSettingsCommand { get; }

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
            if (dmProcess != null)
            {
                dmProcess?.Kill();
                dmProcess?.WaitForExit();
                dmProcess?.Dispose();
            }
        }

        #region private method

        private void OpenSettings()
        {
            var notepad = Process.Start("notepad", LauncherSettings.PATH);
            notepad.EnableRaisingEvents = true;
            notepad.Exited += (s, o) => LauncherSettings.Reload();
        }

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
            dmProcess.StartInfo.CreateNoWindow = false;
            // TODO: dmProcess.StartInfo.Arguments =
            dmProcess.EnableRaisingEvents = true;
            dmProcess.Exited += DmProcessDisposed;
            dmProcess.Disposed += DmProcessDisposed;
            IsRunDm = true;
            dmProcess.Start();
            if (!IsVisibleConsole)
                WindowOperation.Hide(dmProcess.MainWindowHandle);
        }

        private void VisibleConsole()
        {
            if (dmProcess == null)            
                StartDocumentMenagermentProcess();
            IsVisibleConsole = !IsVisibleConsole;
            if (IsVisibleConsole)
                WindowOperation.Show(dmProcess.MainWindowHandle);
            else 
                WindowOperation.Hide(dmProcess.MainWindowHandle);
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

    public static class WindowOperation
    {
        public static void Hide(IntPtr win)
        {
            ShowWindow(win, 0);
        }

        public static void Show(IntPtr win)
        {
            ShowWindow(win, 1);
        }

        // Link: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
    }
}
