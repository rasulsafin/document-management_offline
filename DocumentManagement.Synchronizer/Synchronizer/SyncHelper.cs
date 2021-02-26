using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Synchronizer
{
    public class SyncHelper
    {
        public static async Task<List<SyncAction>> Analysis(RevisionCollection local, RevisionCollection remote, ISynchroTable synchro)
        {
            var result = new List<SyncAction>();
            List<Revision> localRevs = new List<Revision>(synchro.GetRevisions(local));
            List<Revision> remoteRevs = new List<Revision>(synchro.GetRevisions(remote));

            foreach (var localRev in localRevs)
            {
                var remoteRev = remoteRevs.Find(r => r.ID == localRev.ID);
                SyncActionType actionType = (remoteRev == null) ? SyncActionType.Upload
                    : (remoteRev.IsDelete && !localRev.IsDelete) ? SyncActionType.DeleteLocal
                    : (!remoteRev.IsDelete && localRev.IsDelete) ? SyncActionType.DeleteRemote
                    : (remoteRev > localRev) ? SyncActionType.Download
                    : (remoteRev < localRev) ? SyncActionType.Upload
                    : SyncActionType.None;
                SyncAction action = GetAction(localRev, actionType, synchro);

                if (action.TypeAction != SyncActionType.None)
                    result.Add(action);

                if (remoteRev != null)
                    remoteRevs.Remove(remoteRev);
            }

            foreach (var remoteRev in remoteRevs)
            {
                result.Add(GetAction(remoteRev, SyncActionType.Download, synchro));
            }

            return result;

            static SyncAction GetAction(Revision localRev, SyncActionType action, ISynchroTable synchro)
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

            try
            {
                switch (action.TypeAction)
                {
                    case SyncActionType.None:
                        break;
                    case SyncActionType.Download:
                        Revision remoteRev1 = synchro.GetRevisions(remote).Find(r => r.ID == id);
                        await synchro.Download(action);
                        synchro.SetRevision(local, remoteRev1);
                        break;
                    case SyncActionType.Upload:
                        Revision localRev1 = synchro.GetRevisions(local).Find(r => r.ID == id);
                        await synchro.Upload(action);
                        synchro.SetRevision(remote, localRev1);
                        break;
                    case SyncActionType.DeleteLocal:
                        Revision localRev2 = synchro.GetRevisions(local).Find(r => r.ID == id);
                        await synchro.DeleteLocal(action);
                        synchro.SetRevision(remote, localRev2);
                        break;
                    case SyncActionType.DeleteRemote:
                        Revision remoteRev2 = synchro.GetRevisions(remote).Find(r => r.ID == id);
                        await synchro.DeleteRemote(action);
                        synchro.SetRevision(local, remoteRev2);
                        break;
                    case SyncActionType.Special:
                        action.SpecialSynchronization = true;
                        break;
                }
            }
            catch (System.Exception ex)
            {
                action.IsComplete = false;
                action.Data = ex;
            }
        }
    }
}
