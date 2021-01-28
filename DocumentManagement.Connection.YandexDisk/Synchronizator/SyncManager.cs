using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class SyncManager
    {
        private const string REVISIONS = "revisions";
        private DiskManager disk;
        private int total;
        private int current;

        public delegate void ProgressChangeDelegate(int current, int total, string message);

        public event ProgressChangeDelegate ProgressChange;

        public RevisionCollection Revisions { get; private set; } = new RevisionCollection();

        public bool NeedStopSync { get; private set; }
        public bool NowSync { get; set; }

        public async Task Initialize(string accessToken)
        {
            if (disk == null)
            {
                disk = new DiskManager(accessToken);
                await LoadRevisions();
            }
        }

        #region Update Table
        public void Update(TableRevision table, int id, TypeChange type = TypeChange.Update)
        {
            if (type == TypeChange.Update)
                Revisions.GetRevision(table, id).Incerment();
            else if (type == TypeChange.Delete)
                Revisions.GetRevision(table, id).Delete();
            SaveRevisions();
        }

        // public void Update(int id)
        // {
        //    Revisions.GetProject((int)id).Incerment();
        //    SaveRevisions();
        // }
        // public void Update(ID<ProjectDto> id)
        // {
        //    Revisions.GetProject((int)id).Incerment();
        //    SaveRevisions();
        // }
        // public void Delete(ID<ProjectDto> id)
        // {
        //    Revisions.GetProject((int)id).Delete();
        //    SaveRevisions();
        // }
        // public void Delete(ID<UserDto> id)
        // {
        //    Revisions.GetUser((int)id).Delete();
        //    SaveRevisions();
        // }
        // public void Update(ID<ObjectiveDto> id)
        // {
        //    Revisions.GetObjective((int)id).Incerment();
        //    SaveRevisions();
        // }
        // public void Delete(ID<ObjectiveDto> id)
        // {
        //    Revisions.GetObjective((int)id).Delete();
        //    SaveRevisions();
        // }
        // public void Update(ID<ItemDto> id, ID<ProjectDto> idProj)
        // {
        //    Revisions.GetItem((int)id).Incerment();
        //    SaveRevisions();
        // }
        // public void Delete(ID<ItemDto> id, ID<ProjectDto> idProj)
        // {
        //    Revisions.GetItem((int)id).Delete();
        //    SaveRevisions();
        // }
        // public void Update(ID<ItemDto> id, ID<ObjectiveDto> idObj)
        // {
        //    Revisions.GetItem((int)id).Incerment();
        //    SaveRevisions();
        // }
        // public void Delete(ID<ItemDto> id, ID<ObjectiveDto> idObj)
        // {
        //    Revisions.GetItem((int)id).Delete();
        //    SaveRevisions();
        // }

        #endregion

        public void StopSync()
        {
            NeedStopSync = true;
        }

        public async Task StartSync(ProgressChangeDelegate progressChange, DMContext context, IMapper mapper)
        {
            NowSync = true;
            NeedStopSync = false;
            RevisionCollection remote = await disk.Pull<RevisionCollection>(REVISIONS);
            if (remote == null) remote = new RevisionCollection();
            progressChange.Invoke(0, 0, "Analysis");

            var userSynchro = new UserSynchro(disk, context);
            var projectSynchro = new ProjectSynchro(disk, context, mapper);
            var objectiveSynchro = new ObjectiveSynchro(disk, context, mapper);
            var itemSynchro = new ItemSynchro(disk, context);

            List<SyncAction> syncActions = await SyncHelper.Analysis(Revisions, remote, userSynchro);
            var actions = await SyncHelper.Analysis(Revisions, remote, projectSynchro);
            syncActions.AddRange(actions);
            actions = await SyncHelper.Analysis(Revisions, remote, objectiveSynchro);
            syncActions.AddRange(actions);
            actions = await SyncHelper.Analysis(Revisions, remote, itemSynchro);
            syncActions.AddRange(actions);
            total = syncActions.Count;
            current = 0;

            foreach (var action in actions)
            {
                if (NeedStopSync) break;
                ISynchroTable synchro = null;
                if (action.Synchronizer == nameof(UserSynchro)) synchro = userSynchro;
                else if (action.Synchronizer == nameof(ProjectSynchro)) synchro = projectSynchro;
                else if (action.Synchronizer == nameof(ObjectiveSynchro)) synchro = objectiveSynchro;
                else if (action.Synchronizer == nameof(ItemSynchro)) synchro = itemSynchro;
                else throw new NotImplementedException("Синхронизатор не найден");
                await SyncHelper.RunAction(action, synchro, Revisions, remote);
                progressChange.Invoke(current++, total, "Sync");
            }

            progressChange.Invoke(current, total, "Save");
            await disk.Push(remote, REVISIONS);
            SaveRevisions();
            NowSync = false;
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
            string str = JsonConvert.SerializeObject(Revisions, Formatting.Indented);
            File.WriteAllText(fileName, str);
        }
    }

    
}
