#define TEST
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Services;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class SyncManager
    {
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

        private const string REVISIONS = "revisions";
        private const int COUNT_TRY = 3;

        public bool NeedStopSync { get; private set; }

        public bool NowSync { get; set; }

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
                new ObjectiveTypeSynchro(disk, context, mapper),
                new ObjectiveSynchro(disk, context, mapper),
                new ItemSynchro(disk, context),
            };
            List<SyncAction> syncActions = await Analysis(localRevisions, remoteRevisions, synchros);
            if (syncActions.Count > 0)
            {
                progress.total = syncActions.Count;
                progress.current = 0;

                try
                {
                    Console.WriteLine("Начата синхронизация");
                    progress.message = "Sync";
                    for (int i = 0; i < COUNT_TRY; i++)
                    {
                        List<SyncAction> noComplete = new List<SyncAction>();
                        foreach (var action in syncActions)
                        {
                            if (NeedStopSync) break;
                            await FindSyncroRunAction(localRevisions, remoteRevisions, action, synchros);
                            if (action.IsComplete)
                                progress.current++;
                            else
                                noComplete.Add(action);
                            Console.WriteLine($"Синхронизировано элементов: {progress.current}");
                        }

                        if (noComplete.Count == 0) break;

                        syncActions = noComplete;
                    }
                }
                catch (Exception ex)
                {
                    progress.error = ex;
                }
                finally
                {
                    progress.message = "Save";
                    await disk.Push(remoteRevisions, REVISIONS);
                    SaveRevisions();
                    NowSync = false;
                    if (progress.error == null)
                        progress.message = "Complete";
                    else
                        progress.message = "Error";

                    Console.WriteLine("Синхронизация завершена!");
                }
            }
            else
            {
                progress.message = "Not Need";
            }
        }

        private async Task LoadRevisions()
        {
            string fileName = PathManager.GetLocalRevisionFile();
            FileInfo info = new FileInfo(fileName);
            try
            {
                Console.WriteLine($"RevisionFile={info.FullName}");
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
