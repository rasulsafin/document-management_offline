using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using DocumentManagement.Launcher.Base;

namespace DocumentManagement.Launcher.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SelectorWindow.xaml
    /// </summary>
    public partial class SelectorWindow : Window
    {
        public SelectorWindow(SelectorViewModel model)
        {
            InitializeComponent();
            Closing += (e, o) =>
            {
                DialogResult = model.Select != null;
            };
            model.Close += () => Close();
            DataContext = model;
            Loaded += (a, b) =>
            {
                model.Initialize();
            };
        }
    }
}
