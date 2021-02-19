using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexConnection : IConnection
    {
        private const string AUTH_FIELD_KEY_TOKEN = "token";
        private const string NAME_CONNECTION = "Yandex Disk";
        private SyncManager syncManager;

        public YandexConnection() { }

        public YandexConnection(SyncManager syncManager)
        {
            this.syncManager = syncManager;
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            try
            {
                if (await IsAuthDataCorrect(info))
                {

                    YandexDiskAuth auth = new YandexDiskAuth();
                    if (!info.AuthFieldValues.ContainsKey(AUTH_FIELD_KEY_TOKEN))
                    {
                        var tokenNew = await auth.GetYandexDiskToken(info);
                        info.AuthFieldValues.Add(AUTH_FIELD_KEY_TOKEN, tokenNew);
                    }

                    var token = info.AuthFieldValues[AUTH_FIELD_KEY_TOKEN];

                    syncManager.Initialization(new YandexManager(new YandexDiskController(token)));

                    return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.OK, Message = "Good", };
                }

                return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.Error, Message = "Data app not correct", };
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

        public Task<ConnectionInfoDto> UpdateConnectionInfo(ConnectionInfoDto info)
        {
            info.ConnectionType = GetConnectionType();
            return Task.FromResult(info);
        }

        public Task<ProgressSync> GetProgressSyncronization()
        {
            return Task.FromResult(syncManager.GetProgressSync());
        }

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info)
        {
            var manager = syncManager.GetManager() as YandexManager;
            if (manager != null)
            {
                return await manager.GetStatusAsync();
            }

            return new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatusDto.NeedReconnect,
                Message = "Manager null",
            };
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoDto info)
        {
            var connect = info.ConnectionType;
            if (connect.Name == NAME_CONNECTION)
            {
                if (connect.AppProperties.ContainsKey(YandexDiskAuth.KEY_CLIENT_ID) &&
                    connect.AppProperties.ContainsKey(YandexDiskAuth.KEY_RETURN_URL))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
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

        // TODO: Make a proper method.
        public ConnectionTypeDto GetConnectionType()
        {
            var type = new ConnectionTypeDto
            {
                Name = NAME_CONNECTION,
                AuthFieldNames = new List<string>() { "token" },
                AppProperties = new Dictionary<string, string>
                {
                    { YandexDiskAuth.KEY_CLIENT_ID, "b1a5acbc911b4b31bc68673169f57051" },
                    { YandexDiskAuth.KEY_CLIENT_SECRET, "b4890ed3aa4e4a4e9e207467cd4a0f2c" },
                    { YandexDiskAuth.KEY_RETURN_URL, @"http://localhost:8000/oauth/" },
                },
                ObjectiveTypes = new List<ObjectiveTypeDto>()
                {
                    new ObjectiveTypeDto() { Name = "YandexDisk_Issue" },
                },
            };

            return type;
        }

    }
}
