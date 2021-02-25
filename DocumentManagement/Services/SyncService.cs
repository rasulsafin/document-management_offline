﻿//using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Synchronizer.Legacy;

namespace MRS.DocumentManagement.Utility
{
    public class SyncService : ISyncService
    {
        private SyncManager syncManager;

        public SyncService(SyncManager syncManager)
        {
            this.syncManager = syncManager;
        }

        public void Update(NameTypeRevision table, int id, TypeChange type = TypeChange.Update) { } //syncManager?.Update(table, id, type);
    }
}
