using DocumentManagement.Base;
using DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DocumentManagement
{
    internal partial class MainViewModel : BaseViewModel
    {
        private Dispatcher dispatcher;
        static MainViewModel instanse;
        public static MainViewModel Instanse { get => instanse;}

        static YandexDiskController controller;

        private ObservableCollection<DiskElement> folderItems;
        public ObservableCollection<DiskElement> FolderItems
        {
            get { return this.folderItems; }
            set
            {
                this.folderItems = value;
                this.OnPropertyChanged("FolderItems");
            }
        }

        

        public ObservableCollection<ProjectDto> Projects = new ObservableCollection<ProjectDto>();

        public MainViewModel(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            instanse = this;
            Auth.StartAuth();
        }
        private void SetFolderItems(IEnumerable<DiskElement> items)
        {
            FolderItems = new ObservableCollection<DiskElement>(items);
        }

        internal async void SelectItemAsync(int selectedIndex)
        {
            var item = FolderItems[selectedIndex];
            if (item.IsDirectory)
            {
                //this.previousPath = this.currentPath;
                var items = await controller.GetListAsync(item.href);
                SetFolderItems(items);
            }
        }

        internal void SelectionChanged(SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}