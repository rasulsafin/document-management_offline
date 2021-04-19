using System;
using System.Windows;
using DocumentManagement.Launcher.Base;

namespace DocumentManagement.Launcher
{
    public class NotifyIconViewModel : BaseViewModel
    {
        private bool isShowWindow;

        public NotifyIconViewModel()
        {
            ShowWindowCommand = new HCommand(ShowMainWindow);
            ExitApplicationCommand = new HCommand(ExitApplication);
            HideWindowCommand = new HCommand(HideMainWindow);
        }

        public bool IsShowWindow
        {
            get => isShowWindow;
            set
            {
                isShowWindow = value;
                OnPropertyChanged();
            }
        }

        public HCommand ShowWindowCommand { get; }

        public HCommand ExitApplicationCommand { get; }

        public HCommand HideWindowCommand { get; }

        private void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        private void HideMainWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Close();
                IsShowWindow = false;
            }
        }

        private void ShowMainWindow()
        {
            if (Application.Current.MainWindow == null)
            {
                Application.Current.MainWindow = new MainWindow();
            }

            Application.Current.MainWindow.Show();
            IsShowWindow = true;
        }
    }
}
