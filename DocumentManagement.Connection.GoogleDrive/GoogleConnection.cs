using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    public class GoogleConnection : IConnection
    {
        private SyncManager syncManager;

        public GoogleConnection(SyncManager syncManager)
        {
            this.syncManager = syncManager;
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            try
            {
                GoogleDriveController driveController = new GoogleDriveController();
                await driveController.InitializationAsync(info);
                var manager = new GoogleDriveManager(driveController);
                syncManager.Initialization(manager);

                return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.OK, };
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatusDto.Error,
                    Message = ex.Message,
                };
            }
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
            // TODO: Пользователь поменял подключение, в этом случае изменния вступят в силу 
            // только после перезагрузки приловжения
            if (syncManager.Initilize)
            {
                await syncManager.StartSync();
                return true;
            }

            return false;
        }

        public Task<bool> StopSyncronization()
        {
            syncManager.StopSync();
            return Task.FromResult(true);
        }
    }
}
