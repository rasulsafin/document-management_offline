using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Utility
{
    public class SyncService : ISyncService
    {
        private static SyncManager syncManager;

        public SyncService()
        {
            syncManager ??= SyncManager.Instance;
        }

        public void Update(TableRevision table, int id, TypeChange type = TypeChange.Update) => syncManager.Update(table, id, type);
    }
}
