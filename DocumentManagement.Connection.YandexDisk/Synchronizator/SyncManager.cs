﻿using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizator
{
    public class SyncManager
    {
        public delegate void ProgressChangeDelegate(int current, int total, string message);
        public event ProgressChangeDelegate ProgressChange;
        private YandexDiskManager yandex;
        private int total;
        private int current;
        public Revisions Revisions { get; private set; } = new Revisions();

        public async void Initialize(string accessToken)
        {
            if (yandex == null)
            {
                yandex = new YandexDiskManager(accessToken);                
                await LoadRevisions();
            }
        }

        private async Task LoadRevisions()
        {
            string fileName = PathManager.GetLocalRevisionFile();
            try
            {
                string json = await File.ReadAllTextAsync(fileName);
                Revisions = JsonConvert.DeserializeObject<Revisions>(json);
            }
            catch
            {
                Revisions = new Revisions();
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

        public async Task SyncTableAsync(ProgressChangeDelegate progressChange)
        {
            Revisions revisions = await yandex.GetRevisionsAsync();
            total = GetCount(Revisions) + GetCount(revisions);
            Progress<(int, int, string)> progress = new Progress<(int, int, string)>();
            progress.ProgressChanged += (s, p) =>
            {
                (int current, int total, string message) = p;
                progressChange?.Invoke(current, total, message);
            };

            await Synchronize(progress, new UserSynchronizer(yandex), revisions);
            await Synchronize(progress, new ProjectSynchronizer(yandex), revisions);

            await yandex.SetRevisionsAsync(Revisions);
            SaveRevisions();
        }



        private static void CompareRevision(List<int> download, List<int> unload,
            List<Revision> local, List<Revision> remote, IProgress<(int, int, string)> progress)
        {

            foreach (var localRev in local)
            {
                // Находим совподения
                var remoteRev = remote.Find(r => r.ID == localRev.ID);
                if (remoteRev == null)
                {
                    // Загружаем на сервер
                    unload.Add(localRev.ID);
                }
                else if (localRev < remoteRev)
                {
                    // Скачиваем с сервера
                    download.Add(localRev.ID);
                    remote.Remove(remoteRev);
                    localRev.Rev = remoteRev.Rev;
                }
                else if (localRev > remoteRev)
                {
                    // Загружаем на сервер
                    unload.Add(localRev.ID);
                    remote.Remove(remoteRev);
                }
                else if (localRev.Equals(remoteRev))
                {
                    // Пропускаем 
                    // TODO : Продумать как лучше выташить прогрксс
                    //progress.Report(1);
                    remote.Remove(remoteRev);
                }
                //progress.Report(1);
            }
            foreach (var remoteRev in remote)
            {
                // Скачиваем с сервера
                download.Add(remoteRev.ID);
                local.Add(remoteRev);
            }
        }

        private async Task Synchronize(IProgress<(int, int, string)> progress, ISynchronizer synchro, Revisions remoreRevisions)
        {
            List<int> download = new List<int>();
            List<int> unload = new List<int>();
            List<Revision> local = synchro.GetRevisions(Revisions);
            List<Revision> remote = synchro.GetRevisions(remoreRevisions);

            if (local == null) local = new List<Revision>();
            if (remote == null) remote = new List<Revision>();

            CompareRevision(download, unload, local, remote, progress);
            //synchro.SetRevision(Revisions, local);
            synchro.LoadLocalCollect();

            if (download.Count > 0)
            {
                //надо скачать
                foreach (int num in download)
                {
                    if (await synchro.RemoteExistAsync(num))
                    {
                        await synchro.DownloadAndUpdateAsync(num);
                    }
                    else
                    {
                        synchro.DeleteLocal(num);
                    }

                    var subSynchronizes = synchro.GetSubSynchronizes(num);
                    if (subSynchronizes != null)
                    {
                        foreach (var subSynchronize in subSynchronizes)
                        {
                            await Synchronize(progress, subSynchronize, remoreRevisions);
                        }
                    }
                    //progress.Report(1);
                }
            }
            if (unload.Count > 0)
            {
                //надо загрузить
                foreach (int num in unload)
                {
                    if (synchro.LocalExist(num))
                    {
                        await synchro.UpdateRemoteAsync(num);
                    }
                    else
                    {
                        await synchro.DeleteRemoteAsync(num);
                    }

                    var subSynchronizes = synchro.GetSubSynchronizes(num);
                    if (subSynchronizes != null)
                    {
                        foreach (var subSynchronize in subSynchronizes)
                        {
                            await Synchronize(progress, subSynchronize, remoreRevisions);
                        }
                    }
                    //progress.Report(1);
                }
            }
            synchro.SaveLocalCollect();
        }

        private int GetCount(Revisions revisions)
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
