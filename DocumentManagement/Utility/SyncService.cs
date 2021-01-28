using AutoMapper;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Utility
{

    public class SyncService : ISyncService
    {        
        private static SyncManager syncManager;
        private readonly DMContext context;
        private readonly IMapper mapper;
        private int current;
        private int total;
        private string message;
        private static bool initialise = false;

        public SyncService(DMContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
            if (!initialise)
            {
                Initialisation();
            }
        }

        public void Update(TableRevision table, int id, TypeChange type = TypeChange.Update) => syncManager.Update(table, id, type);

        // public void AddChange(ID<ProjectDto> id)
        // {
        //    syncManager.Update(id);
        // }

        // public void AddChange(ID<UserDto> id)
        // {
        //    syncManager.Update(id);
        // }

        // public void AddChange(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        // {
        //    syncManager.Update(id);
        // }

        // public void AddChange(ID<ItemDto> id, ID<ProjectDto> idProj)
        // {
        //    syncManager.Update(id, idProj);
        // }

        // public void AddChange(ID<ItemDto> id, ID<ObjectiveDto> idObj)
        // {
        //    syncManager.Update(id, idObj);
        // }
        public async void StartSync()
        {
            while (!initialise)
            {
                Task.Delay(100);
            }

                if (!syncManager.NowSync)
                await syncManager.StartSync(ProgressChenge, context, mapper);
        }

        public void StopSync() => syncManager.StopSync();

        public (int current, int total, string step) GetSyncProgress() => (current, total, message);

        // public void AddChange(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        // {
        //    syncManager.Update(id, idObj);
        // }
        private void ProgressChenge(int current, int total, string message)
        {
            this.current = current;
            this.total = total;
            this.message = message;
        }

        private static async void Initialisation()
        {
            syncManager = new SyncManager();
            YandexDiskAuth auth = new YandexDiskAuth();
            string accessToken = await auth.GetDiskSdkToken();
            await syncManager.Initialize(accessToken);
            initialise = true;
        }

        // public void Delete(ID<ItemDto> id, ID<ObjectiveDto> idObj)
        // {
        //    syncManager.Delete(id, idObj);
        // }

        // public void Delete(ID<ItemDto> id, ID<ProjectDto> idProj)
        // {
        //    syncManager.Delete(id, idProj);
        // }

        // public void Delete(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        // {
        //    syncManager.Delete(id);
        // }

        // public void Delete(ID<ProjectDto> id)
        // {
        //    syncManager.Delete(id);
        // }

        // public void Delete(ID<UserDto> id)
        // {
        //    syncManager.Delete(id);
        // }
    }

}