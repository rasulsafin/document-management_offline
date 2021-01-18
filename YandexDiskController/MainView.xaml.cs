
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

namespace MRS.DocumentManagement
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
            Closed += MainView_Closed;
            Loaded += Auth.Loaded;
        }

        private void MainView_Closed(object sender, EventArgs e)
        {
            Model.CloseApp();
        }

        private void gridItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => Model.SelectionChanged(e);

        private void gridItems_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Model.SelectItemAsync(this.gridItems.SelectedIndex);

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F7: Model.CreateDirCommand.Execute(null); break;
                case Key.F4: Model.LoadFileCommand.Execute(null); break;
                case Key.F5: Model.RefreshCommand.Execute(null); break;
                case Key.F6: Model.MoveCommand.Execute(null); break;
                case Key.F8: Model.DeleteCommand.Execute(null); break;
                case Key.F11: Model.DebugCommand.Execute(null); break;
                
                default:
                    break;
            }
        }

        private int count=0;
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            count++;
            if (count > 1) 
            {
                // WinBox.ShowMessage($"TabItem_IsEnabledChanged" +
                //    $"\n{e.AddedItems.Count}" +
                //    $"\n{e.RemovedItems.Count}");
            }

            // if (sender is FrameworkElement element)
            // try
            // {
            // WinBox.ShowMessage($"TabItem_IsEnabledChanged");
            // }
            // catch
            // { }
        }

        // private void TabItem_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        // {
        //    WinBox.ShowMessage($"TabItem_IsEnabledChanged");
        // }
    }
}
