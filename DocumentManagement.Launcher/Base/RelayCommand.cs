using System;
using System.Windows.Input;

namespace MRS.DocumentManagement.Launcher.Base
{
    public class RelayCommand : ICommand
    {
        private Action method;

        public RelayCommand(Action method)
        {
            this.method = method;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return method != null;
        }

        public void Execute(object parameter)
        {
                method.Invoke();
        }
    }
}
