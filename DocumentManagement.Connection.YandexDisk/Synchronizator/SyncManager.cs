using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class SyncManager
    {
        private DiskManager diskManager;
        private int total;
        private int current;

        public delegate void ProgressChangeDelegate(int current, int total, string message);

        public event ProgressChangeDelegate ProgressChange;

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

        #region Update Table
        public void Update(ID<ProjectDto> id)
        {
            Revisions.GetProject((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ProjectDto> id)
        {
            Revisions.GetProject((int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<UserDto> id)
        {
            Revisions.GetUser((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<UserDto> id)
        {
            Revisions.GetUser((int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {
            Revisions.GetObjective((int)idProj, (int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {
            Revisions.GetObjective((int)idProj, (int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            Revisions.GetItem((int)idProj, (int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            Revisions.GetItem((int)idProj, (int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {
            Revisions.GetItem((int)idProj, (int)idObj, (int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {
            Revisions.GetItem((int)idProj, (int)idObj, (int)id).Delete();
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

        private Task Synchronize(IProgress<(int, int, string)> progress, ISynchronizer synchro, RevisionCollection remoreRevisions)
        {
            return Task.CompletedTask;
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
    }
}
