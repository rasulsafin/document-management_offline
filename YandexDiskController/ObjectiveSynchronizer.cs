using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement
{
    internal class ObjectiveSynchronizer : ISynchronizer
    {
        private YandexDiskManager yandex;        
        private ProjectDto project;        

        public ObjectiveSynchronizer(YandexDiskManager yandex, ProjectDto localProject)
        {
            this.yandex = yandex;
            this.project = localProject;
        }

        public List<Revision> GetRevision(Revisions revisions)
        {
            throw new System.NotImplementedException();
        }

        public List<ISynchronizer> GetSubSynchronizes(int id)
        {
            throw new System.NotImplementedException();
        }


        public void LoadLocalCollect()
        {
            throw new System.NotImplementedException();
        }
        public void SaveLocalCollect()
        {
            throw new System.NotImplementedException();
        }



        public Task<bool> RemoteExistAsync(int id)
        {
            throw new System.NotImplementedException();
        }
        public Task DownloadAndUpdateAsync(int id)
        {
            throw new System.NotImplementedException();
        }
        public void DeleteLocal(int id)
        {
            throw new System.NotImplementedException();
        }


        public bool LocalExist(int id)
        {
            throw new System.NotImplementedException();
        }
        public Task UpdateRemoteAsync(int id)
        {
            throw new System.NotImplementedException();
        }
        public Task DeleteRemoteAsync(int id)
        {
            throw new System.NotImplementedException();
        }

    }
}