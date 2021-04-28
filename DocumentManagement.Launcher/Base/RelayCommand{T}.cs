using System;
using System.Windows.Input;

namespace MRS.DocumentManagement.Launcher.Base
{
    public class RelayCommand<T> : ICommand
    {
        private Action<T> method;

        public RelayCommand(Action<T> method) => this.method = method;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => method != null;

        public void Execute(object parameter)
        {
            if (parameter is T parameterT)
                method.Invoke(parameterT);
        }
    }
}
