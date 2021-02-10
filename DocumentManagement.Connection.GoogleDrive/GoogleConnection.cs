using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    public class GoogleConnection : IConnection
    {
        SyncManager syncManager;

        public GoogleConnection(SyncManager syncManager)
        {
            this.syncManager = syncManager;
        }

        public async Task<(bool, string)> Connect(dynamic param)
        {
            GoogleDriveController driveController = new GoogleDriveController();
            await driveController.InitializationAsync();

            return (true, string.Empty);
        }

        public Task<ProgressSync> GetProgressSyncronization()
        {
            return Task.FromResult(syncManager.GetProgressSync());
        }

        public Task<bool> IsAuthDataCorrect()
        {
            return Task.FromResult(syncManager.Initilize);
        }

        public async Task<bool> StartSyncronization()
        {
            if (syncManager.Initilize)
            {
                await syncManager.StartSync();
                return true;
            }

            return false;
        }

        public Task StopSyncronization()
        {
            syncManager.StopSync();
            return Task.CompletedTask;
        }
    }
}
