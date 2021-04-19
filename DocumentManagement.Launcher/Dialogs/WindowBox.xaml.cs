using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DocumentManagement.Launcher.Base;

namespace DocumentManagement.Launcher.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class WindowBox : Window
    {
        public WindowBox(InputBoxModel model)
        {
            InitializeComponent();
            model.Success = () =>
            {
                DialogResult = true;
                Close();
            };
            model.Close = () =>
            {
                DialogResult = false;
                Close();
            };
            DataContext = model;
            Loaded += (a, b) => 
            { 
                model.Initialize(); 
            };
        }
    }
}
