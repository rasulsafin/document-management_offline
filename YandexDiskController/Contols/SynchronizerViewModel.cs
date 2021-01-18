using System;
using System.Collections.ObjectModel;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.YandexDisk;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class SynchronizerViewModel : BaseViewModel
    {
        // const string ALL_TRANSACTION = "Все операции";
        // const string NON_SYNC_TRANSACTION = "Не Синхронизированные операции";
        // const string LOCAL_TRANSACTION = "Локальные операции";

        public static SynchronizerViewModel Instanse { get => instanse; }

        public ulong Revision { get; set; }
        private string showAllTransactionContent;// = ALL_TRANSACTION;
        public string ShowAllTransactionContent
        {
            get => showAllTransactionContent; set
        {
            showAllTransactionContent = value;
            OnPropertyChanged();
        }
        }
        public string ProgressText
        {
            get => progressText; private set
        {
            progressText = value;
            OnPropertyChanged();
        }
        }
        public bool SyncProcces
        {
            get => syncProcces; private set
        {
            syncProcces = value;
            OnPropertyChanged();
        }
        }

        private Synchronizer synchronizer;

        public HCommand SynchronizeCommand { get; }
        public HCommand UploadAllCommand { get; }
        public HCommand SynchronizeAllCommand { get; }
        public HCommand DownloadAllCommand { get; }
        public HCommand StopSyncCommand { get; }
        public HCommand RevisionCommand { get; }
        public HCommand ShowAllTransactionCommand { get; }

        private static SynchronizerViewModel instanse;
        private DiskManager yandex;
        private bool syncProcces;
        private string progressText;

        public ObservableCollection<TransactionModel> Transactions { get; set; } = new ObservableCollection<TransactionModel>();


        public SynchronizerViewModel()
        {
            synchronizer = ObjectModel.Synchronizer;

            // synchronizer.TransactionsChange += Synchronizer_TransactionsChange;
            SynchronizeCommand = new HCommand(SynchronizeAsync);
            UploadAllCommand = new HCommand(UploadAll);
            DownloadAllCommand = new HCommand(DownloadAll);
            StopSyncCommand = new HCommand(StopSync);
            RevisionCommand = new HCommand(GetRevision);

            instanse = this;
            Auth.LoadActions.Add(Initialize);
        }

        private void DownloadAll()
        {
            if (SyncProcces)
                WinBox.ShowMessage("Синхронизация уже запущена!");
            foreach (var item in synchronizer.Revisions.Users)
                item.Rev=0;
            foreach (var proj in synchronizer.Revisions.Projects)
            {
                proj.Rev = 0;
                if (proj.Items != null)
                    foreach (var item in proj.Items)
                        item.Rev = 0;
                if (proj.Objectives != null)
                    foreach (var obj in proj.Objectives)
                    {
                        proj.Rev = 0;
                        if (obj.Items != null)
                            foreach (var item in obj.Items)
                                item.Rev = 0;
                    }
            }
            SynchronizeAsync();
        }

        private void StopSync()
        {
            synchronizer.StopSync();
        }

        private void UploadAll()
        {
            if (SyncProcces)
                WinBox.ShowMessage("Синхронизация уже запущена!");
            foreach (var item in synchronizer.Revisions.Users)
                item.Rev++;
            foreach (var proj in synchronizer.Revisions.Projects)
            {
                proj.Rev++;
                if (proj.Items != null)
                    foreach (var item in proj.Items)
                        item.Rev++;
                if (proj.Objectives != null)
                    foreach (var obj in proj.Objectives)
                    {
                        proj.Rev++;
                        if (obj.Items != null)
                            foreach (var item in obj.Items)
                                item.Rev++;
                    }
            }
            SynchronizeAsync();
        }



        private void Initialize(string accessToken)
        {
            synchronizer.Initialize(accessToken);

            // synchronizer.Load();
            // SetLocalTransactionAsync();
        }

        private async void SynchronizeAsync()
        {
            if (SyncProcces)
                WinBox.ShowMessage("Синхронизация уже запущена!");
            try
            {
                SyncProcces = true;
                await synchronizer.SyncTableAsync(ProgressChange);
                SyncProcces = false;

                // synchronizer.Save();
                // SetLocalTransactionAsync();
                WinBox.ShowMessage("Успех. Синхронизация выполнена.");
            }
            catch (ArgumentNullException ane)
            {
                WinBox.ShowMessage("Синхронизация не выполнена. Нет входа в аккаунт:\n" + ane.Message);
            }
            catch (Exception ex)
            {
                WinBox.ShowMessage($"Другая ошибка:{ex.Message}");
            }

        }

        private void ProgressChange(int current, int total, string message)
        {
            ProgressText = $"{current}/{total} {message}";
        }

        private async void GetRevision()
        {
            WinBox.ShowMessage("Эта кнопка болше не работает!");

            // WinBox.ShowMessage((await synchronizer.GetRevisionServerAsync()).ToString());
        }







    }
}