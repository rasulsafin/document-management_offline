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
        private DiskManager disk;
        private int total;
        private int current;

        public delegate void ProgressChangeDelegate(int current, int total, string message);

        public event ProgressChangeDelegate ProgressChange;

        public RevisionCollection Revisions { get; private set; } = new RevisionCollection();

        public bool NeedStopSync { get; private set; }

        public async Task Initialize(string accessToken)
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

        public void Update(ID<ObjectiveDto> id)
        {
            Revisions.GetObjective((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ObjectiveDto> id)
        {
            Revisions.GetObjective((int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            Revisions.GetProject((int)idProj).GetItem((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            Revisions.GetProject((int)idProj).GetItem((int)id).Delete();
            SaveRevisions();
        }

        public void Update(ID<ItemDto> id, ID<ObjectiveDto> idObj)
        {
            Revisions.GetObjective((int)idObj).GetItem((int)id).Incerment();
            SaveRevisions();
        }

        public void Delete(ID<ItemDto> id, ID<ObjectiveDto> idObj)
        {
            Revisions.GetObjective((int)idObj).GetItem((int)id).Delete();
            SaveRevisions();
        }

        #endregion

        public void StopSync()
        {
            NeedStopSync = true;
        }

        public async Task StartSync(ProgressChangeDelegate progressChange, DMContext context)
        {
            RevisionCollection remote = await disk.Pull<RevisionCollection>("revisions");
            var actions = SyncHelper.Analysis(Revisions, remote, new UserSychro(disk, context));
            var actions = SyncHelper.Analysis(Revisions, remote, new ProjectSychro(disk, context));

            
        }

        private Task Synchronize(IProgress<(int, int, string)> progress, ISynchroTable synchro, RevisionCollection remoreRevisions)
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

    public class UserSychro : ISynchroTable
    {
        private DiskManager disk;
        private DMContext context;

        public UserSychro(DiskManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        List<Revision> ISynchroTable.GetRevisions(RevisionCollection revisions)
        {
            if (revisions.Users == null)
                revisions.Users = new List<Revision>();
            return revisions.Users;
        }

        Task<List<ISynchroTable>> ISynchroTable.GetSubSynchroList(int id)
        {
            throw new NotImplementedException();
        }

        void ISynchroTable.SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetUser(rev.ID).Rev = rev.Rev;
        }

        SyncAction ISynchroTable.SpecialSynchronization(SyncAction action)
        {
            return action;
        }
    }
}
