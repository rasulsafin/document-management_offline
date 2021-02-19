#define TEST
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.SyncData;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizer
{
    public class SyncManager
    {
        private const string REVISIONS = "revisions";
        private const int COUNT_TRY = 3;
        public static readonly string YANDEX = "YANDEX";


        private readonly IServiceScopeFactory factoryScope;
        private readonly IMapper mapper;

        #region field
#if TEST
      //  internal
#else
      //  private
#endif
        //ICloudManager disk;

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

        public SyncManager(IMapper mapper, IServiceScopeFactory factory)
        {
            factoryScope = factory;
            this.mapper = mapper;
            LoadRevisions();
        }

        #region property
        public bool NeedStopSync { get; private set; }

        public bool NowSync { get; set; }

        public bool Initilize { get; private set; }
        #endregion

        #region Update Table
        public void Update(NameTypeRevision table, int id, TypeChange type = TypeChange.Update)
        {
            if (type == TypeChange.Update)
                localRevisions.GetRevision(table, id).Increment();
            else if (type == TypeChange.Delete)
                localRevisions.GetRevision(table, id).Delete();
            SaveRevisions();
        }
        #endregion

        public void Initialization(string token)
        {
           // disk = new CloudManager(new YandexDiskController(token));
            Initilize = true;
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

        public void StopSync()
        {
            NeedStopSync = true;
        }

        public ProgressSync GetProgressSync()
        {
            return progress;
        }

        public async Task StartSync()
        {
            DMContext context = factoryScope.CreateScope().ServiceProvider.GetService<DMContext>();

            progress.Current = 0;
            progress.Total = 0;
            progress.Message = "Analysis";
            progress.Error = null;

            NowSync = true;
            NeedStopSync = false;
           // RevisionCollection remoteRevisions = await disk.Pull<RevisionCollection>(REVISIONS);
           // if (remoteRevisions == null) remoteRevisions = new RevisionCollection();
            var synchros = new ISynchroTable[]
            {
               // new UserSynchro(disk, context),
               // new ProjectSynchro(disk, context, mapper),
               // new ObjectiveTypeSynchro(disk, context, mapper),
               // new ObjectiveSynchro(disk, context, mapper),
                // new ItemSynchro(disk, context),
            };
           // List<SyncAction> syncActions = await Analysis(localRevisions, remoteRevisions, synchros);
            //if (syncActions.Count > 0)
            //{
            //    progress.Total = syncActions.Count;
            //    progress.Current = 0;

            //    try
            //    {
            //        progress.Message = "Sync";
            //        for (int i = 0; i < COUNT_TRY; i++)
            //        {
            //            List<SyncAction> noComplete = new List<SyncAction>();
            //            foreach (var action in syncActions)
            //            {
            //                if (NeedStopSync) break;
            //               // await FindSyncroRunAction(localRevisions, remoteRevisions, action, synchros);
            //                if (action.IsComplete)
            //                    progress.Current++;
            //                else
            //                    noComplete.Add(action);
            //            }

            //            if (noComplete.Count == 0) break;

            //            syncActions = noComplete;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        progress.Error = ex;
            //    }
            //    finally
            //    {
            //        progress.Message = "Save";
            //        await disk.Push(remoteRevisions, REVISIONS);
            //        SaveRevisions();
            //        NowSync = false;
            //        if (progress.Error == null)
            //            progress.Message = "Complete";
            //        else
            //            progress.Message = "Error";
            //    }
            //}
            //else
            //{
            //    progress.Message = "Not Need";
            //}
        }

        private void LoadRevisions()
        {
           // string fileName = PathManager.GetLocalRevisionFile();
            //FileInfo info = new FileInfo(fileName);
            //try
            //{
            //    string json = File.ReadAllText(fileName);
            //    localRevisions = JsonConvert.DeserializeObject<RevisionCollection>(json);
            //}
            //catch
            //{
            //    localRevisions = new RevisionCollection();
            //    SaveRevisions();
            //}
        }

        private void SaveRevisions()
        {
            // string fileName = PathManager.GetLocalRevisionFile();
            //string str = JsonConvert.SerializeObject(localRevisions, Formatting.Indented);
            //File.WriteAllText(fileName, str);
        }
    }
}
