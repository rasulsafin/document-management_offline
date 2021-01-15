using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class SyncManager
    {
        public delegate void ProgressChangeDelegate(int current, int total, string message);
        public event ProgressChangeDelegate ProgressChange;
        private DiskManager diskManager;
        private int total;
        private int current;
        public RevisionCollection Revisions { get; private set; } = new RevisionCollection();
        public bool NeedStopSync { get; private set; }

        public async void Initialize(string accessToken)
        {
            if (diskManager == null)
            {
                diskManager = new DiskManager(accessToken);                
                await LoadRevisions();
            }
        }

        private async Task LoadRevisions()
        {
            string fileName = PathManager.GetLocalRevisionFile();
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
            string fileName = PathManager.GetLocalRevisionFile();
            string str = JsonConvert.SerializeObject(Revisions);
            File.WriteAllText(fileName, str);
        }        

        #region Update Table        
        public void Update(ID<ProjectDto> id)
        {            
            Revisions.UpdateProject((int)id);
            SaveRevisions();
        }
        public void Update(ID<UserDto> id)
        {            
            Revisions.UpdateUser((int)id);
            SaveRevisions();
        }
        public void Update(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {            
            Revisions.UpdateObjective((int)idProj, (int)id);
            SaveRevisions();
        }
        public void Update(ID<ItemDto> id, ID<ProjectDto> idProj)
        {            
            Revisions.UpdateItem((int)idProj, (int)id);
            SaveRevisions();
        }
        public void Update(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {            
            Revisions.UpdateItem((int)idProj, (int)idObj, (int)id);
            SaveRevisions();
        }
        #endregion

        public void StopSync()
        {
            NeedStopSync = true;
        }

        public async Task SyncTableAsync(ProgressChangeDelegate progressChange, DMContext context)
        {
            RevisionCollection revisions = await diskManager.GetRevisionsAsync();            
            Progress<(int, int, string)> progress = new Progress<(int, int, string)>();
            progress.ProgressChanged += (s, p) =>
            {
                (int current, int total, string message) = p;
                progressChange?.Invoke(current, total, message);
            };

            await Synchronize(progress, new UserSynchronizer(diskManager, context), revisions);
            await Synchronize(progress, new ProjectSynchronizer(diskManager, context), revisions);

            await diskManager.SetRevisionsAsync(Revisions);
            SaveRevisions();
        }



        private async Task Synchronize(IProgress<(int, int, string)> progress, ISynchronizer synchro, RevisionCollection remoreRevisions)
        {
            progress.Report((current, Revisions.Total, "Подготовка данных"));
            //List<int> download = new List<int>();
            //List<int> unload = new List<int>();

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
                        await synchro.DownloadAndUpdateAsync(localRev.ID);
                        await SubSinchronize(progress, synchro, remoreRevisions, localRev);
                    }
                    else
                    {
                        await synchro.DeleteLocalAsync(localRev.ID);
                    }
                    if (!NeedStopSync)
                        synchro.SetRevision(Revisions, remoteRev);

                    remote.Remove(remoteRev);
                    //localRev.Rev = remoteRev.Rev;
                }
                else if (localRev > remoteRev)
                {
                    // Загружаем на сервер

                    if (await synchro.LocalExist(localRev.ID))
                    {
                        await synchro.UpdateRemoteAsync(localRev.ID);
                        await SubSinchronize(progress, synchro, remoreRevisions, localRev);
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
                    await SubSinchronize(progress, synchro, remoreRevisions, remoteRev);
                }
                else
                {
                    await synchro.DeleteLocalAsync(remoteRev.ID);
                }
                progress.Report((++current, Revisions.Total, "Загрузка"));

                if (!NeedStopSync)
                    synchro.SetRevision(Revisions, remoteRev);
                //local.Add(remoteRev);
            }
            progress.Report((current, Revisions.Total, "Сохранение результатов"));
            await synchro.SaveLocalCollectAsync();
        }

        private async Task SubSinchronize(IProgress<(int, int, string)> progress, ISynchronizer synchro, RevisionCollection remoreRevisions, Revision localRev)
        {
            var subSynchronizes = await synchro.GetSubSynchronizesAsync(localRev.ID);
            if (subSynchronizes != null)
            {
                foreach (var subSynchronize in subSynchronizes)
                {
                    await Synchronize(progress, subSynchronize, remoreRevisions);
                }
            }
        }

        private int GetCount(RevisionCollection revisions)
        {
            int? result = 0;
            result += revisions.Projects?.Count + revisions.Users?.Count;
            result += revisions.Projects?.Sum(x => x.Objectives?.Count);
            result += revisions.Projects?.Sum(x => x.Items?.Count);
            result += revisions.Projects?.Sum(x => x.Objectives?.Sum(q => q.Items?.Count));
            return result ?? 0;
        }
    }
}
