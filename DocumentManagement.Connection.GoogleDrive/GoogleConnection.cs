using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    public class GoogleConnection : IConnection
    {
        private const string NAME_CONNECT = "Google Drive";
        // private SyncManager syncManager;
        private ConnectionInfoDto connectionInfo;
        private GoogleDriveManager manager;

        public GoogleConnection()
        {
        }

        //public GoogleConnection(SyncManager syncManager)
        //{
        //    this.syncManager = syncManager;
        //}

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            try
            {
                connectionInfo = info;
                GoogleDriveController driveController = new GoogleDriveController();
                await driveController.InitializationAsync(connectionInfo);
                manager = new GoogleDriveManager(driveController);
                //syncManager.Initialization(manager);

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

        public ConnectionTypeDto GetConnectionType()
        {
            var type = new ConnectionTypeDto
            {
                Name = NAME_CONNECT,
                AuthFieldNames = new List<string>
                {
                    "token",
                },
                AppProperties = new Dictionary<string, string>
                {
                    { GoogleDriveController.APPLICATION_NAME, "BRIO MRS" },
                    { GoogleDriveController.CLIENT_ID, "1827523568-ha5m7ddtvckjqfrmvkpbhdsl478rdkfm.apps.googleusercontent.com" },
                    { GoogleDriveController.CLIENT_SECRET, "fA-2MtecetmXLuGKXROXrCzt" },
                },
            };

            return type;
        }

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info)
        {
            //GoogleDriveManager manager = syncManager.GetManager() as GoogleDriveManager;
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
            if (connect.Name == NAME_CONNECT)
            {
                if (connect.AppProperties.ContainsKey(GoogleDriveController.APPLICATION_NAME) &&
                    connect.AppProperties.ContainsKey(GoogleDriveController.CLIENT_ID) &&
                    connect.AppProperties.ContainsKey(GoogleDriveController.CLIENT_SECRET))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public Task<bool> StartSyncronization()
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopSyncronization()
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionInfoDto> UpdateConnectionInfo(ConnectionInfoDto info)
        {
            info.AuthFieldValues = connectionInfo.AuthFieldValues;
            info.ConnectionType = GetConnectionType();
            return Task.FromResult(info);
        }
    }
}
