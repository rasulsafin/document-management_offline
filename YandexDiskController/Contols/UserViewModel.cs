using DocumentManagement.Base;
using DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.ObjectModel;

namespace DocumentManagement.Contols
{
    public class UserViewModel : BaseViewModel
    {
        private static readonly string DIR_NAME = "data";
        private static readonly string FILE_NAME = "users.xml";
        private static readonly string TEMP_DIR = "Temp.Yandex";
        YandexDisk yandex;

        public ObservableCollection<UserDto> Users { get; set; } = new ObservableCollection<UserDto>();

    }
}