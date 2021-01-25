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

    public class SyncHelper
    {
        public static List<SyncAction> Analysis(RevisionCollection local, RevisionCollection remote, ISynchroTable synchro)
        {
            var result = new List<SyncAction>();
            List<Revision> localRevs = synchro.GetRevisions(local);
            List<Revision> remoteRevs = synchro.GetRevisions(remote);

            foreach (var localRev in localRevs)
            {
                SyncAction action = null;
                var remoteRev = remoteRevs.Find(r => r.ID == localRev.ID);
                if (remoteRev == null)
                {
                    action = GetAction(localRev, TypeSyncAction.Upload, synchro);
                }
                else if (remoteRev.IsDelete && !localRev.IsDelete)
                {
                    action = GetAction(localRev, TypeSyncAction.DeleteLocal, synchro);
                }
                else if (!remoteRev.IsDelete && localRev.IsDelete)
                {
                    action = GetAction(localRev, TypeSyncAction.DeleteRemote, synchro);
                }
                else if (remoteRev > localRev)
                {
                    action = GetAction(localRev, TypeSyncAction.Download, synchro);
                }
                else if (remoteRev < localRev)
                {
                    action = GetAction(localRev, TypeSyncAction.Upload, synchro);
                }
                else
                {
                    action = GetAction(localRev, TypeSyncAction.None, synchro);
                }

                action = synchro.SpecialSynchronization(action);
                result.Add(action);

                if (remoteRev != null)
                    remoteRevs.Remove(remoteRev);
            }

            foreach (var remoteRev in remoteRevs)
            {
                result.Add(GetAction(remoteRev, TypeSyncAction.Download, synchro));
            }

            return result;

            static SyncAction GetAction(Revision localRev, TypeSyncAction action, ISynchroTable synchro)
            {
                return new SyncAction()
                {
                    ID = localRev.ID,
                    TypeAction = action,
                    Synchronizer = synchro.GetType().Name,
                };
            }
        }

        public static async Task RunAction(SyncAction action,
            ISynchroTable synchro,
            RevisionCollection local,
            RevisionCollection remote)
        {
            int id = action.ID;

            switch (action.TypeAction)
            {
                case TypeSyncAction.None:
                    break;
                case TypeSyncAction.Download:
                    Revision remoteRev1 = synchro.GetRevisions(remote).Find(r => r.ID == id);
                    await synchro.Download(id);
                    synchro.SetRevision(local, remoteRev1);
                    break;
                case TypeSyncAction.Upload:
                    Revision localRev1 = synchro.GetRevisions(local).Find(r => r.ID == id);
                    await synchro.Upload(id);
                    synchro.SetRevision(remote, localRev1);
                    break;
                case TypeSyncAction.DeleteLocal:
                    Revision localRev2 = synchro.GetRevisions(local).Find(r => r.ID == id);
                    await synchro.DeleteLocal(id);
                    synchro.SetRevision(remote, localRev2);
                    break;
                case TypeSyncAction.DeleteRemote:
                    Revision remoteRev2 = synchro.GetRevisions(remote).Find(r => r.ID == id);
                    await synchro.DeleteRemote(id);
                    synchro.SetRevision(local, remoteRev2);
                    break;
                case TypeSyncAction.Special:
                    action.SpecialSynchronization = true;
                    break;
            }

            if (action.SpecialSynchronization)
            {
                var subSynchroList = await synchro.GetSubSynchroList(action.ID);
                // TODO Выполнить внутренюю синхронизация
                await synchro.Special(action);
            }
        }
    }
}