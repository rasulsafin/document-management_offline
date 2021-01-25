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
        #region fieled
        private static SynchronizerViewModel instanse;
        private string showAllTransactionContent; // = ALL_TRANSACTION;
        private Synchronizer synchronizer;
        private DiskManager yandex;
        private bool syncProcces;
        private string progressText;

        #endregion
        #region constructors
        public SynchronizerViewModel()
        {
            synchronizer = ObjectModel.Synchronizer;

            // synchronizer.TransactionsChange += Synchronizer_TransactionsChange;
            SynchronizeCommand = new HCommand(SynchronizeAsync);
            UploadAllCommand = new HCommand(UploadAll);
            DownloadAllCommand = new HCommand(DownloadAll);
            StopSyncCommand = new HCommand(StopSync);
            RevisionCommand = new HCommand(GetRevision);
            UpdateRevCommand = new HCommand(UpdateRev);

            instanse = this;
            Auth.LoadActions.Add(Initialize);
        }

        #endregion
        #region property
        public static SynchronizerViewModel Instanse { get => instanse; }

        public ulong Revision { get; set; }

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

        public HCommand SynchronizeCommand { get; }

        public HCommand UploadAllCommand { get; }

        public HCommand SynchronizeAllCommand { get; }

        public HCommand DownloadAllCommand { get; }

        public HCommand StopSyncCommand { get; }

        public HCommand RevisionCommand { get; }

        public HCommand UpdateRevCommand { get; }

        public HCommand ShowAllTransactionCommand { get; }

        public ObservableCollection<TransactionModel> Transactions { get; set; } = new ObservableCollection<TransactionModel>();

        #endregion
        #region private method
        private void UpdateRev()
        {
            // void RevCheck(Connection.Synchronizator.Revision rev)
            // {
            //    if (rev.Rev == 0) rev.Incerment();
            // }

            // var revs = ObjectModel.Synchronizer.Revisions;
            // foreach (var item in ObjectModel.Users)
            // {
            //    var rev = revs.GetUser(item.ID);
            //    RevCheck(rev);
            // }

            // foreach (var project in ObjectModel.Projects)
            // {
            //    var revP = revs.GetProject(project.ID);
            //    RevCheck(revP);
            //    foreach (var item in ObjectModel.GetItems(project.dto))
            //    {
            //        var rev = revP.FindItem((int)item.ID);
            //        RevCheck(rev);
            //    }

            // foreach (var obj in ObjectModel.GetObjectives(project.dto))
            //    {
            //        var revO = revP.FindObjetive((int)obj.ID);
            //        RevCheck(revO);

            // foreach (var item in ObjectModel.GetItems(project.dto, obj.ID))
            //        {
            //            var rev = revO.FindItem((int)item.ID);
            //            RevCheck(rev);
            //        }
            //    }
            // }

            // ObjectModel.Synchronizer.SaveRevisions();
        }

        private void DownloadAll()
        {
            // if (SyncProcces)
            //    WinBox.ShowMessage("Синхронизация уже запущена!");
            // foreach (var item in synchronizer.Revisions.Users)
            //    item.Rev = 0;
            // foreach (var proj in synchronizer.Revisions.Projects)
            // {
            //    proj.Rev = 0;
            //    if (proj.Items != null)
            //    {
            //        foreach (var item in proj.Items)
            //            item.Rev = 0;
            //    }

            // if (proj.Objectives != null)
            //    {
            //        foreach (var obj in proj.Objectives)
            //        {
            //            proj.Rev = 0;
            //            if (obj.Items != null)
            //            {
            //                foreach (var item in obj.Items)
            //                    item.Rev = 0;
            //            }
            //        }
            //    }
            // }

            // SynchronizeAsync();
        }

        private void StopSync()
        {
            synchronizer.StopSync();
        }

        private void UploadAll()
        {
            // if (SyncProcces)
            //    WinBox.ShowMessage("Синхронизация уже запущена!");
            // foreach (var item in synchronizer.Revisions.Users)
            //    item.Rev++;
            // foreach (var proj in synchronizer.Revisions.Projects)
            // {
            //    proj.Rev++;
            //    if (proj.Items != null)
            //    {
            //        foreach (var item in proj.Items)
            //            item.Rev++;
            //    }

            // if (proj.Objectives != null)
            //    {
            //        foreach (var obj in proj.Objectives)
            //        {
            //            proj.Rev++;
            //            if (obj.Items != null)
            //            {
            //                foreach (var item in obj.Items)
            //                    item.Rev++;
            //            }
            //        }
            //    }
            // }

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
                WinBox.ShowMessage($"Другая ошибка:{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ProgressChange(int current, int total, string message)
        {
            ProgressText = $"{current}/{total} {message}";
        }

        private void GetRevision()
        {
            WinBox.ShowMessage("Эта кнопка болше не работает!");

            // WinBox.ShowMessage((await synchronizer.GetRevisionServerAsync()).ToString());
        }
        #endregion
    }
}
