using DocumentManagement.Base;
using DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.ObjectModel;

namespace DocumentManagement
{
    internal partial class MainModel : BaseViewModel
    {
        static YandexDiskController controller;

        public ObservableCollection<ProjectDto> Projects = new ObservableCollection<ProjectDto>();

        public MainModel()
        {
            Auth.StartAuth();
            
        }


    }
}