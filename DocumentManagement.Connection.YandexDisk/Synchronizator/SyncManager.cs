﻿#define TEST
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
#if TEST
        internal
#else
        private
#endif
        DiskManager disk;

#if TEST
        internal
#else
        private
#endif
        ProgressSync progress;

#if TEST
        internal
#else
        private
#endif
        RevisionCollection localRevisions = new RevisionCollection();

        public delegate void ProgressChangeDelegate(int current, int total, string message);

        public event ProgressChangeDelegate ProgressChange;


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
                localRevisions.GetRevision(table, id).Incerment();
            else if (type == TypeChange.Delete)
                localRevisions.GetRevision(table, id).Delete();
            SaveRevisions();
        }
#endregion

        public void StopSync()
        {
            NeedStopSync = true;
        }

        public ProgressSync GetProgressSync()
        {
            return progress;
        }

        public async Task StartSync(DMContext context, IMapper mapper)
        {
            //ProgressChangeDelegate progressChange,
            progress.current = 0;
            progress.total = 0;
            progress.message = "Analysis";
            progress.error = null;

            NowSync = true;
            NeedStopSync = false;
            RevisionCollection remoteRevisions = await disk.Pull<RevisionCollection>(REVISIONS);
            if (remoteRevisions == null) remoteRevisions = new RevisionCollection();
            var synchros = new ISynchroTable[]
            {
                new UserSynchro(disk, context),
                new ProjectSynchro(disk, context, mapper),
                new ObjectiveSynchro(disk, context, mapper),
                new ItemSynchro(disk, context),
            };
            List<SyncAction> syncActions = await Analysis(localRevisions, remoteRevisions, synchros);
            progress.total = syncActions.Count;
            progress.current = 0;

            try
            {
                progress.message = "Sync";
                foreach (var action in syncActions)
                {
                    if (NeedStopSync) break;
                    await FindSyncroRunAction(localRevisions, remoteRevisions, action, synchros);
                    progress.current++;
                }
            }
            catch (Exception ex)
            {
                progress.error = ex;
            }

            progress.message = "Save";
            await disk.Push(remoteRevisions, REVISIONS);
            SaveRevisions();
            NowSync = false;
        }

        public static async Task FindSyncroRunAction(
            RevisionCollection local,
            RevisionCollection remote,
            SyncAction action,
            IEnumerable<ISynchroTable> synchros)
        {
            foreach (var synchro in synchros)
            {
                if (action.Synchronizer == synchro.GetType().Name)
                {
                    await SyncHelper.RunAction(action, synchro, local, remote);
                    break;
                }
            }
        }

        public static async Task<List<SyncAction>> Analysis(
            RevisionCollection local,
            RevisionCollection remote,
            IEnumerable<ISynchroTable> synchros)
        {
            List<SyncAction> syncActions = new List<SyncAction>();
            foreach (var synchro in synchros)
            {
                // TODO: функция обхода всей базы нужна не всегда
                synchro.CheckDBRevision(local);
                var actions = await SyncHelper.Analysis(local, remote, synchro);
                syncActions.AddRange(actions);
            }
            return syncActions;
        }

        private async Task LoadRevisions()
        {
            string fileName = PathManager.GetLocalRevisionFile();
            try
            {
                string json = await File.ReadAllTextAsync(fileName);
                localRevisions = JsonConvert.DeserializeObject<RevisionCollection>(json);
            }
            catch
            {
                localRevisions = new RevisionCollection();
                SaveRevisions();
            }
        }

        private void SaveRevisions()
        {
            string fileName = PathManager.GetLocalRevisionFile();
            string str = JsonConvert.SerializeObject(localRevisions, Formatting.Indented);
            File.WriteAllText(fileName, str);
        }
    }


}
