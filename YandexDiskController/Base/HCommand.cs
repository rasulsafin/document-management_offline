using System;
using System.Windows;
using System.Windows.Input;

namespace WPFStorage.Base
{
    public class HCommand : ICommand
    {
        private Action _method;

        public HCommand(Action method)
        {
            _method = method;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _method != null;
        }

        public void Execute(object parameter)
        {
            try
            {
                _method.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка метода '{_method.Method.Name}', модуля '{_method.Target.GetType().Name}'. {ex.Message}");
            }
        }
    }

    public class HCommand<T> : ICommand
    {
        private Action<T> _method;

        public HCommand(Action<T> method)
        {
            _method = method;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _method != null;
        }

        public void Execute(object parameter)
        {
            try
            {
                if (parameter is T parameterT) _method.Invoke(parameterT);
                else if (parameter is string s_parameter)
                {
                    System.Reflection.ParameterInfo[] pars = _method.Method.GetParameters();
                    if (pars.Length != 1) throw new ArgumentException("Количество параметров больше 1!");
                    var par = pars[0];
                    if (par.ParameterType == typeof(int)
                        && int.TryParse(s_parameter, out int i_parameter)
                        && i_parameter is T par_t)
                        _method.Invoke(par_t);
                    else if (par.ParameterType == typeof(double)
                        && double.TryParse(s_parameter.Replace(',', '.'), out double d_parameter)
                        && d_parameter is T par_t2)
                        _method.Invoke(par_t2);
                    else
                        MessageBox.Show($"не удалось привести параметр {parameter} к типу { (typeof(T)).FullName }");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка метода '{_method.Method.Name}', модуля '{_method.Target.GetType().Name}'. {ex.Message}");
            }
        }
    }
}
