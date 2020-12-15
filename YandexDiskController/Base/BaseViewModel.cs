using System.ComponentModel;

namespace DocumentManagement.Base
{
    /// <summary>
    /// https://github.com/hty007/testTask/blob/master/GPSTask/BaseView/BaseViewModel.cs
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}