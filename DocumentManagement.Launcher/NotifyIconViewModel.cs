using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using MRS.DocumentManagement.Launcher.Base;

namespace MRS.DocumentManagement.Launcher
{
    public class NotifyIconViewModel : ObservableObject, IDisposable
    {
        #region field
        private bool isDMRunning;
        private Process dmProcess;
        private bool isConsoleVisible = false;
        #endregion

        #region constructor
        public NotifyIconViewModel()
        {
            this.VisibleConsoleCommand = new RelayCommand(this.VisibleConsole);
            ExitApplicationCommand = new RelayCommand(ExitApplication);
            OpenSwaggerCommand = new RelayCommand(OpenSwagger);
            StartDmConsoleCommand = new RelayCommand(StartDocumentMenagement);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            StartDocumentMenagement();
        }

        #endregion

        #region binding
        public bool IsDMRunning
        {
            get => isDMRunning;
            set => SetProperty(ref isDMRunning, value);
        }

        public bool IsConsoleVisible
        {
            get => isConsoleVisible;
            set => SetProperty(ref isConsoleVisible, value);
        }

        public RelayCommand ExitApplicationCommand { get; }

        public RelayCommand VisibleConsoleCommand { get; }

        public RelayCommand OpenSwaggerCommand { get; }

        public RelayCommand StartDmConsoleCommand { get; }

        public RelayCommand OpenSettingsCommand { get; }

        #endregion

        public void Dispose()
        {
            if (dmProcess != null)
            {
                dmProcess.Kill();
                dmProcess.WaitForExit();
                dmProcess.Dispose();
            }
        }

        #region private method
        private static void Hide(IntPtr win) => SafeNativeMethods.ShowWindow(win, 0);

        private static void Show(IntPtr win) => SafeNativeMethods.ShowWindow(win, 1);

        private void OpenSettings()
        {
            var notepad = Process.Start("notepad", LauncherSettings.PATH);
            notepad.EnableRaisingEvents = true;
            notepad.Exited += (s, o) => LauncherSettings.Load();
        }

        private void OpenSwagger() => OpenUrl(LauncherSettings.SwaggerPath);

        private void DmProcessDisposed(object sender, EventArgs e) => IsDMRunning = false;

        private void StartDocumentMenagement()
        {
            dmProcess = Process.GetProcessesByName("DocumentManagement.Api").FirstOrDefault();
            if (dmProcess != null)
            {
                dmProcess.Kill();
                dmProcess.WaitForExit();
            }

            string path = LauncherSettings.DMExecutablePath;
            if (!File.Exists(path))
            {
                MessageBox.Show($"Файл не найден!\n{path}");
                return;
            }

            var executableDir = Path.GetDirectoryName(path);

            dmProcess = new Process();
            dmProcess.StartInfo.FileName = path;
            dmProcess.StartInfo.CreateNoWindow = false;
            dmProcess.StartInfo.UseShellExecute = false;
            dmProcess.StartInfo.WorkingDirectory = executableDir;
            dmProcess.StartInfo.Arguments = string.Join(" ", Environment.GetCommandLineArgs());
            dmProcess.EnableRaisingEvents = true;
            dmProcess.Exited += DmProcessDisposed;
            IsDMRunning = true;
            dmProcess.Start();

            // Waiting for the console window to open
            while (dmProcess.MainWindowHandle == IntPtr.Zero)
            {
            }

            if (!IsConsoleVisible)
                Hide(dmProcess.MainWindowHandle);
        }

        private void VisibleConsole()
        {
            if (dmProcess == null)
                StartDocumentMenagement();
            IsConsoleVisible = !IsConsoleVisible;
            if (IsConsoleVisible)
                Show(dmProcess.MainWindowHandle);
            else
                Hide(dmProcess.MainWindowHandle);
        }

        private void ExitApplication() => Application.Current.Shutdown();

        private void OpenUrl(string url)
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
        }
        #endregion
    }
}
