﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.GoogleDrive.Synchronization;
using MRS.DocumentManagement.Connection.Utils.CloudBase;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    public class GoogleConnection : IConnection
    {
        private const string NAME_CONNECT = "Google Drive";
        private ConnectionInfoExternalDto connectionInfo;
        private static GoogleDriveManager manager;

        public GoogleConnection()
        {
        }

        public async Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
        {
            try
            {
                connectionInfo = info;
                GoogleDriveController driveController = new GoogleDriveController();
                await driveController.InitializationAsync(connectionInfo);
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

        public async Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            // TODO: fix this.
            return await Connect(info, default);
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
            var objectiveType = "GoogleDriveIssue";
            info.ConnectionType.ObjectiveTypes = new List<ObjectiveTypeExternalDto>
            {
                new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
            };

            if (string.IsNullOrWhiteSpace(info.UserExternalID))
                info.UserExternalID = Guid.NewGuid().ToString();

            return Task.FromResult(info);
        }

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            var connectResult = await Connect(info, default);
            if (connectResult.Status != RemoteConnectionStatus.OK || manager == null)
                return null;

            return GoogleDriveConnectionContext.CreateContext(manager);
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            await Connect(info, default);
            return new CommonConnectionStorage(manager);
        }
    }
}
