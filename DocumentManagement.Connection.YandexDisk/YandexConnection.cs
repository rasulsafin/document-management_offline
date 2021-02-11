using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexConnection : IConnection
    {
        private SyncManager syncManager;

        public YandexConnection(SyncManager syncManager)
        {
            this.syncManager = syncManager;
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            try
            {
                YandexDiskAuth auth = new YandexDiskAuth();
                var token = await auth.GetYandexDiskToken();

                syncManager.Initialization(new YandexManager(new YandexDiskController(token)));

                return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.OK };
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

        public Task<bool> StopSyncronization()
        {
            syncManager.StopSync();
            return Task.FromResult(true);
        }
    }
}
