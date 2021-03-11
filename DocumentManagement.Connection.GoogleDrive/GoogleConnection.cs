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
        private ConnectionInfoExternalDto connectionInfo;
        private GoogleDriveManager manager;

        public GoogleConnection()
        {
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info)
        {
            try
            {
                GoogleDriveController driveController = new GoogleDriveController();
                await driveController.InitializationAsync(info);
                manager = new GoogleDriveManager(driveController);

                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK, Message = "Good", };
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = ex.Message,
                };
            }
        }

        public ConnectionTypeExternalDto GetConnectionType()
        {
            var type = new ConnectionTypeExternalDto
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

        public Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info, DateTime lastSynchronizationDate)
        {
            throw new NotImplementedException();
        }

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            return await Connect(info);
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoExternalDto info)
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

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            info.AuthFieldValues = connectionInfo.AuthFieldValues;
            return Task.FromResult(info);
        }
    }
}
