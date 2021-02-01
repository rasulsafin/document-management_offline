using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private static bool initialise = false;
        private readonly IMapper mapper;
        private readonly IServiceScopeFactory factoryScope;

        public SyncService(IMapper mapper, IServiceScopeFactory factory)
        {
            this.mapper = mapper;
            factoryScope = factory;
            if (!initialise)
            {
                Initialisation();
            }
        }

        public void Update(TableRevision table, int id, TypeChange type = TypeChange.Update) => syncManager.Update(table, id, type);

        public async void StartSync()
        {
            while (!initialise)
            {
                await Task.Delay(10);
            }

            var cont = factoryScope.CreateScope().ServiceProvider.GetService<DMContext>();
            if (!syncManager.NowSync)
                await syncManager.StartSync(cont, mapper);
        }

        public void StopSync() => syncManager.StopSync();

        public ProgressSync GetProgress() => syncManager.GetProgressSync();

        private static async void Initialisation()
        {
            syncManager = new SyncManager();
            YandexDiskAuth auth = new YandexDiskAuth();
            string accessToken = await auth.GetDiskSdkToken();
            await syncManager.Initialize(accessToken);
            initialise = true;
        }
    }
}