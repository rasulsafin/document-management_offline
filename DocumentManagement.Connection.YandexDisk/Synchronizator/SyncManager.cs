#define TEST
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class SyncManager
    {
        private const string REVISIONS = "revisions";
        private const int COUNT_TRY = 3;
        public static readonly string YANDEX = "YANDEX";
        private static SyncManager instance;

        #region field
#if TEST
        internal
#else
        private
#endif
        IDiskManager disk;

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

        #endregion

        public SyncManager()
        {
            LoadRevisions();
        }

        #region property
        public static SyncManager Instance { get => instance ??= new SyncManager(); }

        public bool NeedStopSync { get; private set; }

        public bool NowSync { get; set; }

        public bool Initilize { get; private set; }
        #endregion

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

        public void Initialization(RemoteConnectionInfoDto connection)
        {
            // Selecting a third-party document flow
            if (connection.ServiceName == YANDEX)
            {
                if (connection.AuthFieldNames == null || connection.AuthFieldNames.Count() == 0)
                {
                    YandexHelper.OpenBrowser(YandexDiskAuth.OAUTH_URL);
                }
                else
                {
                    disk = new DiskManager(new YandexDiskController(connection.AuthFieldNames.First()));
                    Initilize = true;
                }
            }
        }

        private void LoadRevisions()
        {
            string fileName = PathManager.GetLocalRevisionFile();
            FileInfo info = new FileInfo(fileName);
            try
            {
                Console.WriteLine($"RevisionFile={info.FullName}");
                string json = File.ReadAllText(fileName);
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
