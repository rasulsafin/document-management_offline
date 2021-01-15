using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizator
{
    internal class ProjectSynchronizer : ISynchronizer
    {
        private DiskManager yandex;

        public ProjectSynchronizer(DiskManager yandex)
        {
            this.yandex = yandex;
        }

        public void DeleteLocal(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteRemoteAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task DownloadAndUpdateAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
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

        public bool LocalExist(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RemoteExistAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public void SaveLocalCollect()
        {
            throw new System.NotImplementedException();
        }

        public void SetRevision(RevisionCollection revisions, List<Revision> local)
        {
            throw new System.NotImplementedException();
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateRemoteAsync(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}