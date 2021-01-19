using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement
{
    public class ItemsSynchronizer : ISynchronizer
    {
        private DiskManager yandex;
        private ProjectDto remoteProject;
        private ProjectDto localProject;
        private ObjectiveDto remoteObj;
        private ObjectiveDto localObj;
        private List<ItemDto> remoteItems;
        private List<ItemDto> localItems;
        private ItemDto remoteItem;
        private ItemDto localItem;
        private bool toObjective;

        public ItemsSynchronizer(DiskManager yandex, ProjectDto remoteProject, ProjectDto localProject)
        {
            this.yandex = yandex;
            this.remoteProject = remoteProject;
            this.localProject = localProject;
            toObjective = false;
        }

        public ItemsSynchronizer(DiskManager yandex, ProjectDto actualProject, ObjectiveDto remoteObj, ObjectiveDto localObj)
        {
            this.yandex = yandex;
            this.remoteProject = actualProject;
            this.localProject = actualProject;
            this.remoteObj = remoteObj;
            this.localObj = localObj;
            toObjective = true;
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            var idPro = (int)remoteProject.ID;
            var projectRev = revisions.Projects.Find(pr => pr.ID == idPro);
            if (projectRev == null)
                return new List<Revision>();

            if (remoteObj == null)
            {
                if (projectRev.Items == null)
                    projectRev.Items = new List<Revision>();
                return projectRev.Items;
            }
            else
            {
                if (projectRev.Objectives == null)
                    projectRev.Objectives = new List<ObjectiveRevision>();
                return projectRev.Objectives.Select(x => (Revision)x).ToList();
            }
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            int idProj = (int)remoteProject.ID;
            var projectRev = revisions.Projects.Find(x => x.ID == idProj);

            if (remoteObj == null)
            {
                CopyRevision(rev, projectRev);
            }
            else
            {
                int idObj = (int)remoteObj.ID;
                var index = projectRev.Objectives.FindIndex(x => x.ID == idObj);
                if (index < 0)
                    projectRev.Objectives.Add(new ObjectiveRevision(idObj) { Items = new List<Revision>() { rev } });
                else
                    CopyRevision(rev, projectRev.Objectives[index]);
            }

            void CopyRevision(Revision rev, ObjectiveRevision revision)
            {
                if (revision.Items == null)
                    revision.Items = new List<Revision>();

                var index = revision.Items.FindIndex(x => x.ID == rev.ID);
                if (index < 0)
                    revision.Items.Add(new Revision(rev.ID, rev.Rev));
                else
                    revision.Items[index].Rev = rev.Rev;
            }
        }

        public Task<List<ISynchronizer>> GetSubSynchronizesAsync(int id) => null;

        public void LoadLocalCollect()
        {
            if (toObjective)
            {
                remoteItems = remoteObj?.Items?.ToList();
                localItems = localObj?.Items?.ToList();
            }
            else
            {
                remoteItems = remoteProject?.Items?.ToList();
                localItems = localProject?.Items?.ToList();
            }

            if (remoteItems == null) remoteItems = new List<ItemDto>();
            if (localItems == null) localItems = new List<ItemDto>();
        }

        public Task SaveLocalCollectAsync()
        {
            if (toObjective)
            {
                if (localObj.Items == null) localObj.Items = new List<ItemDto>();
                localObj.Items = localItems;
            }
            else
            {
                if (localProject.Items == null) localProject.Items = new List<ItemDto>();
                localProject.Items = localItems;
            }

            return Task.CompletedTask;
        }

        public Task<bool> RemoteExist(int id)
        {
            Donwload(id);
            return Task.FromResult(remoteItem != null);
        }

        public async Task DownloadAndUpdateAsync(int id)
        {
            Donwload(id);
            var path = PathManager.GetProjectDir(remoteProject);
            await yandex.DownloadItemFile(remoteItem, path);
            localItems.Add(remoteItem);
        }

        public Task DeleteLocalAsync(int id)
        {
            var id1 = (ID<ItemDto>)id;
            localItems.RemoveAll(x => x.ID == id1);
            return Task.CompletedTask;
        }

        public Task<bool> LocalExist(int id)
        {
            Find(id);
            return Task.FromResult(localItem != null);
        }

        public async Task UpdateRemoteAsync(int id)
        {
            Find(id);
            try
            {
                // TODO: Проверить файл ли это
                // Загружаем сам файл
                await yandex.UnloadFileItem(remoteProject, localItem);
            }
            catch (FileNotFoundException)
            {
            }
        }

        public Task DeleteRemoteAsync(int id)
        {
            var id1 = (ID<ItemDto>)id;
            remoteItems.RemoveAll(x => x.ID == id1);
            return Task.CompletedTask;
        }

        private void Donwload(int id)
        {
            var id1 = (ID<ItemDto>)id;
            if (remoteItem?.ID != id1)
                remoteItem = remoteItems.First(x => x.ID == id1);
        }

        private void Find(int id)
        {
            var id1 = (ID<ItemDto>)id;
            if (localItem?.ID != id1)
                localItem = localItems.Find(x => x.ID == id1);
        }
    }
}
