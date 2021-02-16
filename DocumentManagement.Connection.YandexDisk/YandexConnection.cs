﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexConnection : IConnection
    {
        SyncManager syncManager;

        public YandexConnection() { }

        public YandexConnection(SyncManager syncManager)
        {
            this.syncManager = syncManager;
        }

        public async Task<(bool, string)> Connect(dynamic param)
        {
            YandexDiskAuth auth = new YandexDiskAuth();
            var token = await auth.GetYandexDiskToken();
            syncManager.Initialization(token);

            return (true, token);
        }

        public Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionInfoDto> UpdateConnectionInfo(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }

        public Task<ProgressSync> GetProgressSyncronization()
        {
            return Task.FromResult(syncManager.GetProgressSync());
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoDto info)
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

        // TODO: Make a proper method.
        public ConnectionTypeDto GetConnectionType()
        {
            var type = new ConnectionTypeDto
            {
                Name = "yandexdisk",
                AuthFieldNames = new List<string>() { },
                AppProperty = new Dictionary<string, string>
                {
                    { "CLIENT_ID", "b1a5acbc911b4b31bc68673169f57051" },
                    { "CLIENT_SECRET", "b4890ed3aa4e4a4e9e207467cd4a0f2c" },
                    { "RETURN_URL", @"http://localhost:8000/oauth/" },
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
