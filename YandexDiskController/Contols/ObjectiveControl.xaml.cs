using System.Windows.Controls;

namespace MRS.DocumentManagement.Contols
{
    /// <summary>
    /// Логика взаимодействия для ObjectiveControl.xaml
    /// </summary>
    public partial class ObjectiveControl : UserControl
    {
        public ObjectiveControl()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ObjectiveViewModel model)
            {
                model.SelectedObjective = (Models.ObjectiveModel)e.NewValue;
            }
        }
    }
}
