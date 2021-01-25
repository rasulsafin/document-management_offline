using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement
{
    /// <summary>
    /// Код от сюда перекачует в SyncManager
    /// </summary>
    public class Synchronizer
    {
        public delegate void ProgressChangeDelegate(int current, int total, string message);

        public event ProgressChangeDelegate ProgressChange;

        private DiskManager disk;
        private int total;
        private int current;

        public RevisionCollection Revisions { get; private set; } = new RevisionCollection();

        public bool Syncing { get; private set; }

        public bool NeedStopSync { get; private set; }

        public async void Initialize(string accessToken)
        {
            if (disk == null)
            {
                disk = new DiskManager(accessToken);
                await LoadRevisions();
            }
        }

        public void SaveRevisions()
        {
            string dirName = PathManager.GetRevisionsDir();
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = PathManager.GetRevisionFile();
            string str = JsonConvert.SerializeObject(Revisions, Formatting.Indented);
            File.WriteAllText(fileName, str);
        }

        private async Task LoadRevisions()
        {
            string fileName = PathManager.GetRevisionFile();
            try
            {
                string json = await File.ReadAllTextAsync(fileName);
                Revisions = JsonConvert.DeserializeObject<RevisionCollection>(json);
            }
            catch
            {
                Revisions = new RevisionCollection();
                Properties.Settings.Default.ProjectNextId = 1;
                Properties.Settings.Default.UserNextId = 1;
                Properties.Settings.Default.ObjectiveNextId = 1;
                Properties.Settings.Default.ItemNextId = 1;
                Properties.Settings.Default.Save();
                SaveRevisions();
            }
        }

        #region Update Table
        public void Update(ID<ProjectDto> id)
        {
            if (Syncing) return;
            Revisions.GetProject((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ProjectDto> id)
        {
            if (Syncing) return;
            Revisions.GetProject((int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<UserDto> id)
        {
            if (Syncing) return;
            Revisions.GetUser((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<UserDto> id)
        {
            if (Syncing) return;
            Revisions.GetUser((int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.GetObjective((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.GetObjective((int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.GetProject((int)idProj).GetItem( (int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.GetProject((int)idProj).GetItem((int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.GetObjective((int)idObj).GetItem((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.GetObjective((int)idObj).GetItem((int)id).Delete();
            SaveRevisions();
        }

        #endregion

        public async Task SyncTableAsync(ProgressChangeDelegate progressChange)
        {
            // if (yandex == null)
            // {
            //    progressChange?.Invoke(-1, -1, "Не возможно выполнить синхронизацию");
            //    return;
            // }

            // RevisionCollection revisions = await yandex.GetRevisionsAsync();
            // total = 0; // GetCount(Revisions) + GetCount(revisions);
            // current = 0; // GetCount(Revisions) + GetCount(revisions);
            // Progress<(int, int, string)> progress = new Progress<(int, int, string)>();
            // progress.ProgressChanged += (s, p) =>
            // {
            //    (int c, int t, string m) = p;
            //    progressChange?.Invoke(c, t, m);
            // };

            // NeedStopSync = false;

            // try
            // {
            //    await Synchronize(progress, new UserSynchronizer(yandex), revisions);
            //    await Synchronize(progress, new ProjectSynchronizer(yandex), revisions);
            // }
            // catch (Exception)
            // {
            //    throw;
            // }
            // finally
            // {
            //    ((IProgress<(int, int, string)>)progress).Report((current, Revisions.Total, "Сохранение результатов"));
            //    // await yandex.SetRevisionsAsync(revisions);
            //    SaveRevisions();
            // }
        }

        public void StopSync()
        {
            NeedStopSync = true;
        }

        private async Task Synchronize(IProgress<(int, int, string)> progress, ISynchroTable synchro, RevisionCollection remoteRevisions)
        {
            //// progress.Report((current, Revisions.Total, "Подготовка данных"));
            // progress.Report((current, Revisions.Total, "Синхронизация"));

            // List<Revision> local = synchro.GetRevisions(Revisions);
            // List<Revision> remote = synchro.GetRevisions(remoteRevisions);

            // total += local.Count;
            // total += remote.Count;
            // if (Revisions.Total < total) Revisions.Total = total;
            // if (local == null) local = new List<Revision>();
            // if (remote == null) remote = new List<Revision>();

            // await synchro.LoadCollection();
            // foreach (var localRev in local)
            // {
            //    if (NeedStopSync) break;
            //    var remoteRev = remote.Find(r => r.ID == localRev.ID);
            //    await SyncSingleRevision(progress, synchro, remoteRevisions, localRev, remoteRev);
            //    if (remote.Remove(remoteRev))
            //        current++;
            // }

            // foreach (var remoteRev in remote)
            // {
            //    if (NeedStopSync) break;
            //    var localRev = local.Find(r => r.ID == remoteRev.ID);
            //    await SyncSingleRevision(progress, synchro, remoteRevisions, localRev, remoteRev);
            // }

            // progress.Report((current, Revisions.Total, "Синхронизация"));
            // await synchro.SaveCollectionAsync();
        }

        private async Task SyncSingleRevision(IProgress<(int, int, string)> progress, ISynchroTable synchro, RevisionCollection remoteRevisions, Revision localRev, Revision remoteRev)
        {
            //// Синхронизация одной записи
            // TypeSyncAction action = await synchro.GetActoin(localRev, remoteRev);
            // switch (action)
            // {
            //    case TypeSyncAction.None:
            //        break;

            // case TypeSyncAction.Download:
            //        await DownloadAction(progress, synchro, remoteRevisions, remoteRev);
            //        break;

            // case TypeSyncAction.Upload:
            //        await UploadAction(progress, synchro, remoteRevisions, localRev);
            //        break;

            // case TypeSyncAction.DeleteLocal:
            //        await DeleteAction(progress, synchro, remoteRevisions, localRev, remoteRev);
            //        break;
            //    //case TypeSyncAction.SubSync:
            //    //    await SubSynchronize(progress, synchro, remoteRevisions, localRev);
            //    //    break;
            //    default:
            //        break;
            // }

            // progress.Report((++current, Revisions.Total, "Завершено"));
        }

        private async Task DownloadAction(IProgress<(int, int, string)> progress,
            ISynchroTable synchro,
            RevisionCollection remoteRevisions,
            Revision remoteRev)
        {
            //// Скачиваем с сервера
            // progress.Report((current, Revisions.Total, $"Скачивание {synchro.NameElement}"));
            // await synchro.DownloadRemote(remoteRev.ID);
            // await SubSynchronize(progress, synchro, remoteRevisions, remoteRev);
            // if (!NeedStopSync)
            //    synchro.SetRevision(Revisions, remoteRev);
        }

        private async Task UploadAction(IProgress<(int, int, string)> progress,
            ISynchroTable synchro,
            RevisionCollection remoteRevisions,
            Revision localRev)
        {
            // Загружаем на сервер
            // progress.Report((current, Revisions.Total, $"Загрузка {synchro.NameElement}"));
            // await SubSynchronize(progress, synchro, remoteRevisions, localRev);
            // await synchro.UploadLocal(localRev.ID);
            // if (!NeedStopSync)
            //    synchro.SetRevision(remoteRevisions, localRev);
        }

        private async Task DeleteAction(IProgress<(int, int, string)> progress, ISynchroTable synchro, RevisionCollection remoteRevisions, Revision localRev, Revision remoteRev)
        {
            // progress.Report((current, Revisions.Total, $"Удаление {synchro.NameElement}"));
            // if (!localRev.IsDelete)
            // {
            //    await synchro.DeleteLocal(localRev.ID);
            //    synchro.SetRevision(Revisions, remoteRev);
            // }

            // if (!remoteRev.IsDelete)
            // {
            //    await synchro.DeleteRemote(localRev.ID);
            //    synchro.SetRevision(remoteRevisions, remoteRev);
            // }
        }

        private async Task SubSynchronize(IProgress<(int, int, string)> progress, ISynchroTable synchro, RevisionCollection remoteRevisions, Revision localRev)
        {
            var subSynchronizes = await synchro.GetSubSynchroList(localRev.ID);
            if (subSynchronizes != null)
            {
                foreach (var subSynchronize in subSynchronizes)
                {
                    if (NeedStopSync) break;
                    await Synchronize(progress, subSynchronize, remoteRevisions);
                }
            }
        }
    }
}
