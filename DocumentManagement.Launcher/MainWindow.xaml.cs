using System.Diagnostics;
using System.IO;
using System.Windows;
using DocumentManagement.Launcher.Dialogs;

namespace DocumentManagement.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process doc;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartDmapi(object sender, RoutedEventArgs e)
        {
            var path = Properties.Settings.Default.DMApiPath;
            if (!File.Exists(path))
            {
                WinBox.ShowMessage($"Файл не найден!\n{path}");
                return;
            }

            doc = new Process();
            doc.StartInfo.FileName = path;
            doc.StartInfo.CreateNoWindow = true;
            doc.Start();
        }

        private void KillDmapi(object sender, RoutedEventArgs e)
        {
            //doc.
            // doc?.CloseMainWindow();
            // doc?.Close();
            doc?.Kill();
            // doc?.Dispose();
        }
    }
}
