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

    }
}
