using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{

    public class GoogleConnection : IConnection
    {
        SyncManager syncManager;
        ICloudManager manager;

        public GoogleConnection(SyncManager syncManager)
        {
            this.syncManager = syncManager;
        }


        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            GoogleDriveController driveController = new GoogleDriveController();
            await driveController.InitializationAsync();
            manager = new GoogleDriveManager(driveController);

            return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.OK, };
        }

        public Task<ProgressSync> GetProgressSyncronization()
        {
            return Task.FromResult(syncManager.GetProgressSync());
        }

        public Task<ConnectionStatusDto> GetStatus()
        {
            throw new NotImplementedException();
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
