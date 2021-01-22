using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{

    public class SyncService : Interface.Services.ISyncService
    {        
        private readonly DMContext context;
        private static SyncManager SyncManager;
        private int Current;
        private int Total;
        private string Message;
        private static bool initialise = false;

        public SyncService(DMContext context)
        {
            this.context = context;
            if (!initialise)
            {
                Initialisation();
            }
        }

        private static async void Initialisation()
        {
            SyncManager = new SyncManager();
            YandexDiskAuth auth = new YandexDiskAuth();
            string accessToken = await auth.GetDiskSdkToken();
            await SyncManager.Initialize(accessToken);
            initialise = true;
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
            SyncManager.Update(id);
        }

        public void AddChange(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            SyncManager.Update(id, idProj);
        }

        public void AddChange(ID<ItemDto> id, ID<ObjectiveDto> idObj)
        {
            SyncManager.Update(id, idObj);
        }

        public async void StartSyncAsync() => await SyncManager.StartSync(progressChenge, context);

        public void StopSyncAsync() => SyncManager.StopSync();

        private void progressChenge(int current, int total, string message)
        {
            Current = current;
            Total = total;
            Message = message;
        }

        public (int current, int total, string step) GetSyncProgress() => (Current, Total, Message);
    }

}