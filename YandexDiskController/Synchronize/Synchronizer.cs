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

        private DiskManager yandex;
        private YandexDiskController controller;
        private int total;
        private int current;

        public RevisionCollection Revisions { get; private set; } = new RevisionCollection();

        public async void Initialize(string accessToken)
        {
            if (yandex == null)
            {
                yandex = new DiskManager(accessToken);
                controller = new YandexDiskController(accessToken);
                yandex.TempDir = PathManager.TEMP_DIR;

                // await Task.Delay(5000);
                await LoadRevisions();
            }
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
                SaveRevisions();
            }
        }

        private void SaveRevisions()
        {
            string dirName = PathManager.GetRevisionsDir();
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = PathManager.GetRevisionFile();
            string str = JsonConvert.SerializeObject(Revisions, Formatting.Indented);
            File.WriteAllText(fileName, str);
        }

        #region Update Table
        public bool Syncing { get; private set; }

        public bool NeedStopSync { get; private set; }

        public void Update(ID<ProjectDto> id)
        {
            if (Syncing) return;
            Revisions.UpdateProject((int)id);
            SaveRevisions();
        }

        public void Update(ID<UserDto> id)
        {
            if (Syncing) return;
            Revisions.UpdateUser((int)id);
            SaveRevisions();
        }

        public void Update(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.UpdateObjective((int)idProj, (int)id);
            SaveRevisions();
        }

        public void Update(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.UpdateItem((int)idProj, (int)id);
            SaveRevisions();
        }

        public void Update(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.UpdateItem((int)idProj, (int)idObj, (int)id);
            SaveRevisions();
        }
        #endregion

        public async Task SyncTableAsync(ProgressChangeDelegate progressChange)
        {
            RevisionCollection revisions = await yandex.GetRevisionsAsync();
            total = 0; // GetCount(Revisions) + GetCount(revisions);
            current = 0; // GetCount(Revisions) + GetCount(revisions);
            Progress<(int, int, string)> progress = new Progress<(int, int, string)>();
            progress.ProgressChanged += (s, p) =>
            {
                (int c, int t, string m) = p;
                progressChange?.Invoke(c, t, m);
            };

            NeedStopSync = false;

            await Synchronize(progress, new UserSynchronizer(yandex), revisions);
            await Synchronize(progress, new ProjectSynchronizer(yandex), revisions);

            await yandex.SetRevisionsAsync(Revisions);
            SaveRevisions();
        }

        public void StopSync()
        {
            NeedStopSync = true;
        }

        private async Task Synchronize(IProgress<(int, int, string)> progress, ISynchronizer synchro, RevisionCollection remoreRevisions)
        {
            progress.Report((current, Revisions.Total, "Подготовка данных"));

            // List<int> download = new List<int>();
            // List<int> unload = new List<int>();

            List<Revision> local = synchro.GetRevisions(Revisions);
            List<Revision> remote = synchro.GetRevisions(remoreRevisions);

            total += local.Count;
            total += remote.Count;
            if (Revisions.Total < total) Revisions.Total = total;
            if (local == null) local = new List<Revision>();
            if (remote == null) remote = new List<Revision>();

            synchro.LoadLocalCollect();
            progress.Report((current, Revisions.Total, "Подготовка завершена"));
            foreach (var localRev in local)
            {
                progress.Report((current, Revisions.Total, "Загрузка"));
                if (NeedStopSync) break;

                // Находим совподения
                var remoteRev = remote.Find(r => r.ID == localRev.ID);
                if (remoteRev == null)
                {
                    // Загружаем на сервер
                    await synchro.UpdateRemoteAsync(localRev.ID);
                    var subSynchronizes = await synchro.GetSubSynchronizesAsync(localRev.ID);
                    if (subSynchronizes != null)
                    {
                        foreach (var subSynchronize in subSynchronizes)
                        {
                            await Synchronize(progress, subSynchronize, remoreRevisions);
                        }
                    }
                }
                else if (localRev < remoteRev)
                {
                    // Скачиваем с сервера
                    if (await synchro.RemoteExist(localRev.ID))
                    {
                        var subSynchronizes = await synchro.GetSubSynchronizesAsync(localRev.ID);
                        if (subSynchronizes != null)
                        {
                            foreach (var subSynchronize in subSynchronizes)
                            {
                                await Synchronize(progress, subSynchronize, remoreRevisions);
                            }
                        }

                        await synchro.DownloadAndUpdateAsync(localRev.ID);
                    }
                    else
                    {
                        synchro.DeleteLocalAsync(localRev.ID);
                    }

                    if (!NeedStopSync)
                        synchro.SetRevision(Revisions, remoteRev);

                    remote.Remove(remoteRev);

                    // localRev.Rev = remoteRev.Rev;
                }
                else if (localRev > remoteRev)
                {
                    // Загружаем на сервер

                    if (await synchro.LocalExist(localRev.ID))
                    {
                        var subSynchronizes = await synchro.GetSubSynchronizesAsync(localRev.ID);
                        if (subSynchronizes != null)
                        {
                            foreach (var subSynchronize in subSynchronizes)
                            {
                                await Synchronize(progress, subSynchronize, remoreRevisions);
                            }
                        }

                        await synchro.UpdateRemoteAsync(localRev.ID);

                    }
                    else
                    {
                        await synchro.DeleteRemoteAsync(localRev.ID);
                    }

                    remote.Remove(remoteRev);
                }
                else if (localRev.Equals(remoteRev))
                {
                    // Пропускаем
                    remote.Remove(remoteRev);
                }

                progress.Report((++current, Revisions.Total, "Загрузка"));
            }

            foreach (var remoteRev in remote)
            {
                if (NeedStopSync) break;

                // Скачиваем с сервера
                if (await synchro.RemoteExist(remoteRev.ID))
                {
                    await synchro.DownloadAndUpdateAsync(remoteRev.ID);

                    var subSynchronizes = await synchro.GetSubSynchronizesAsync(remoteRev.ID);
                    if (subSynchronizes != null)
                    {
                        foreach (var subSynchronize in subSynchronizes)
                        {
                            await Synchronize(progress, subSynchronize, remoreRevisions);
                        }
                    }
                }
                else
                {
                    await synchro.DeleteLocalAsync(remoteRev.ID);
                }

                progress.Report((++current, Revisions.Total, "Загрузка"));

                if (!NeedStopSync)
                    synchro.SetRevision(Revisions, remoteRev);

                // local.Add(remoteRev);
            }

            progress.Report((current, Revisions.Total, "Сохранение результатов"));
            await synchro.SaveLocalCollectAsync();
        }

        // private static void CompareRevision(List<int> download, List<int> unload,
        //    List<Revision> local, List<Revision> remote/*, IProgress<int> progress*/)
        // {

        // foreach (var localRev in local)
        //    {
        //        if (NeedStopSync) break;
        //        // Находим совподения
        //        var remoteRev = remote.Find(r => r.ID == localRev.ID);
        //        if (remoteRev == null)
        //        {
        //            // Загружаем на сервер
        //            unload.Add(localRev.ID);
        //        }
        //        else if (localRev < remoteRev)
        //        {
        //            // Скачиваем с сервера
        //            download.Add(localRev.ID);
        //            remote.Remove(remoteRev);
        //            localRev.Rev = remoteRev.Rev;
        //        }
        //        else if (localRev > remoteRev)
        //        {
        //            // Загружаем на сервер
        //            unload.Add(localRev.ID);
        //            remote.Remove(remoteRev);
        //        }
        //        else if (localRev.Equals(remoteRev))
        //        {
        //            // Пропускаем
        //            //progress.Report(1);
        //            remote.Remove(remoteRev);
        //        }
        //        //progress.Report(1);
        //    }
        //    foreach (var remoteRev in remote)
        //    {
        //        if (NeedStopSync) break;
        //        // Скачиваем с сервера
        //        download.Add(remoteRev.ID);
        //        local.Add(remoteRev);
        //    }
        // }

        // private int GetCount(Revisions revisions)
        // {
        //    int? result = 0;
        //    result += revisions.Projects?.Count + revisions.Users?.Count;
        //    result += revisions.Projects?.Sum(x => x.Objectives?.Count);
        //    result += revisions.Projects?.Sum(x => x.Items?.Count);
        //    result += revisions.Projects?.Sum(x => x.Objectives?.Sum(q => q.Items?.Count));
        //    return result ?? 0;
        // }
    }
}
