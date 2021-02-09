﻿using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexConnection : IConnection
    {
        SyncManager syncManager;

        public YandexConnection(SyncManager syncManager)
        {
            this.syncManager = syncManager;
        }

        public async Task<ConnectionStatusDto> Connect(dynamic param)
        {
            YandexDiskAuth auth = new YandexDiskAuth();
            var token = await auth.GetYandexDiskToken();
            syncManager.Initialization(token);

            return (true, token);
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
