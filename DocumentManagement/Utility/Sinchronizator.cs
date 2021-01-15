using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizator;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using System;

namespace MRS.DocumentManagement.Utility
{
    public class Sinchronizator
    {
        private readonly DMContext context;
        private readonly SyncManager SyncManager;
        private int Current;
        private int Total;
        private string Message;

        public Sinchronizator(DMContext context)
        {
            this.context = context;
            SyncManager = new SyncManager();
            Initialisation();
        }

        private async void Initialisation()
        {
            YandexDiskAuth auth = new YandexDiskAuth();
            string accessToken = await auth.GetDiskSdkToken();
            SyncManager.Initialize(accessToken);            
        }

        public void AddChange(ID<ProjectDto> id)
        {
            SyncManager.Update(id);
        }

        public void AddChange(ID<UserDto> id)
        {
            SyncManager.Update(id);
        }

        public void AddChange(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {
            SyncManager.Update(id,idProj);
        }

        public void AddChange(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            SyncManager.Update(id, idProj);
        }

        public void AddChange(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {
            SyncManager.Update(id, idObj,idProj);
        }

        public async void StartSyncAsync()
        {
            await SyncManager.SyncTableAsync(progressChenge, context);
        }

        private void progressChenge(int current, int total, string message)
        {
            Current = current;
            Total = total;
            Message = message;
        }
    }

}