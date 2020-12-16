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

namespace DocumentManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private MainViewModel Model { get; }

        public MainView()
        {
            InitializeComponent();
            DataContext = Model = new MainViewModel(this.Dispatcher);
        }

        private void gridItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => Model.SelectionChanged(e);

        private void gridItems_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Model.SelectItemAsync(this.gridItems.SelectedIndex);
    }
}
