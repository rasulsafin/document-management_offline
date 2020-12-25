using WPFStorage.Dialogs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MRS.DocumentManagement.Contols
{
    /// <summary>
    /// Логика взаимодействия для ProjectControl.xaml
    /// </summary>
    public partial class ProjectControl : UserControl
    {
        private ProjectViewModel Model;

        public ProjectControl()
        {
            InitializeComponent();
            Loaded += ProjectControl_Loaded;
        }

        private void ProjectControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProjectViewModel model)
            {
                //WinBox.ShowMessage("Загрузился. Модель есть!");
                Model = model;
            }
            //else 
            //{
            //    WinBox.ShowMessage("Загрузился. Модель нет!");
            //}
        }
    }
}
