using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class SynchronizerViewModel : BaseViewModel
    {
        public static SynchronizerViewModel Instanse { get => instanse; }

        public ulong Revision { get; set; }

        private Synchronizer synchronizer;

        public HCommand SynchronizeCommand { get; }
        public HCommand RevisionCommand { get; }

        private static SynchronizerViewModel instanse;
        private YandexDiskManager yandex;

        public ObservableCollection<TransactionModel> Transactions { get; set; }

        public SynchronizerViewModel()
        {
            synchronizer = ObjectModel.Synchronizer;
            Transactions = synchronizer.Transactions;
            SynchronizeCommand = new HCommand(SynchronizeAsync);
            RevisionCommand = new HCommand(GetRevision);

            instanse = this;
            synchronizer.Load();
        }
        private async void SynchronizeAsync()
        {
            synchronizer.SynchronizeAsync();            
            synchronizer.Save();
            WinBox.ShowMessage("Синхронизация произведена!");
        }

        private async void GetRevision()
        {           
            
            WinBox.ShowMessage(await synchronizer.GetRevisionServerAsync());
        }


        

        

        
    }
}