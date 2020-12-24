using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MRS.DocumentManagement.Base
{
    /// <summary>
    /// https://github.com/hty007/testTask/blob/master/GPSTask/BaseView/BaseViewModel.cs
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}