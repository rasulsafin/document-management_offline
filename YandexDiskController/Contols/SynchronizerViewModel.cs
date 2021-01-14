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
        const string ALL_TRANSACTION = "Все операции";
        const string NON_SYNC_TRANSACTION = "Не Синхронизированные операции";
        const string LOCAL_TRANSACTION = "Локальные операции";

        public static SynchronizerViewModel Instanse { get => instanse; }

        public ulong Revision { get; set; }
        private string showAllTransactionContent = ALL_TRANSACTION;
        public string ShowAllTransactionContent { get => showAllTransactionContent; set { showAllTransactionContent = value; OnPropertyChanged(); } }
        public string ProgressText{get => progressText; private set{progressText = value;OnPropertyChanged();}}
        public bool SyncProcces{get => syncProcces; private set{syncProcces = value;OnPropertyChanged();}}

        private Synchronizer synchronizer;

        public HCommand SynchronizeCommand { get; }
        public HCommand SynchronizeAllCommand { get; }
        public HCommand RevisionCommand { get; }
        public HCommand ShowAllTransactionCommand { get; }

        private static SynchronizerViewModel instanse;
        private YandexDiskManager yandex;
        private bool syncProcces;
        private string progressText;

        public ObservableCollection<TransactionModel> Transactions { get; set; } = new ObservableCollection<TransactionModel>();


        public SynchronizerViewModel()
        {
            synchronizer = ObjectModel.Synchronizer;
            //synchronizer.TransactionsChange += Synchronizer_TransactionsChange;
            SynchronizeCommand = new HCommand(SynchronizeAsync);
            SynchronizeAllCommand = new HCommand(SynchronizeAll);
            RevisionCommand = new HCommand(GetRevision);
            
            instanse = this;
            Auth.LoadActions.Add(Initialize);
        }

        private void SynchronizeAll()
        {
            foreach (var item in synchronizer.Revisions.Users)
                item.Rev++;
            foreach (var proj in synchronizer.Revisions.Projects)
            {
                proj.Rev++;
                foreach (var item in proj.Items)
                    item.Rev++;
                foreach (var obj in proj.Objectives)
                {
                    proj.Rev++;
                    foreach (var item in proj.Items)
                        item.Rev++;
                }
            }
            SynchronizeAsync();
        }

        

        private void Initialize(string accessToken)
        {
            synchronizer.Initialize(accessToken);
            //synchronizer.Load();
            //SetLocalTransactionAsync();
        }

        private async void SynchronizeAsync()
        {
                //WinBox.ShowMessage("Синхронизация более не существует!");
            try
            {
                SyncProcces = true;
                await synchronizer.SyncTableAsync(ProgressChange);
                SyncProcces = false;
                //synchronizer.Save();
                //SetLocalTransactionAsync();
                WinBox.ShowMessage("Успех. Синхронизация выполнена.");
            }
            catch (ArgumentNullException ane)
            {
                WinBox.ShowMessage("Синхронизация не выполнена. Нет входа в аккаунт:\n"+ane.Message);
            }
            catch (Exception ex)
            {
                WinBox.ShowMessage($"Другая ошибка:{ex.Message}");
            }

        }

        private void ProgressChange(int current, int total)
        {
            ProgressText = $"{current}/{total}";
        }

        private async void GetRevision()
        {
            WinBox.ShowMessage("Эта кнопка болше не работает!");
            //WinBox.ShowMessage((await synchronizer.GetRevisionServerAsync()).ToString());
        }







    }
}