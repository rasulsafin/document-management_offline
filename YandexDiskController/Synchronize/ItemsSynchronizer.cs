using System;
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
        private DiskManager disk;
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
            this.disk = yandex;
            this.remoteProject = remoteProject;
            this.localProject = localProject;
            toObjective = false;
        }

        public ItemsSynchronizer(DiskManager yandex, ProjectDto actualProject, ObjectiveDto remoteObj, ObjectiveDto localObj)
        {
            this.disk = yandex;
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

            if (!toObjective)
            {
                if (projectRev.Items == null)
                    projectRev.Items = new List<Revision>();
                return projectRev.Items;
            }
            else
            {
                int idObj = (int)remoteObj.ID;
                if (projectRev.Objectives == null)
                    projectRev.Objectives = new List<ObjectiveRevision>();
                var objective = projectRev.Objectives.Find(x => x.ID == idObj);
                if (objective == null)
                    return new List<Revision>();
                if (objective.Items == null)
                    objective.Items = new List<Revision>();
                return objective.Items;
            }
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            int idProj = (int)remoteProject.ID;
            var projectRev = revisions.Projects.Find(x => x.ID == idProj);

            if (!toObjective)
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

        public Task<List<ISynchronizer>> GetSubSynchronizesAsync(int id)
        {
            return Task.FromResult<List<ISynchronizer>>(null);
        }

        public void LoadCollection()
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

        public async Task<SyncAction> GetActoin(Revision localRev, Revision remoteRev)
        {
            if (localRev == null) localRev = new Revision(remoteRev.ID);
            if (remoteRev == null) remoteRev = new Revision(localRev.ID);
            if (localRev.IsDelete || remoteRev.IsDelete) return SyncAction.Delete;

            FindRemote(localRev.ID);
            FindLocal(localRev.ID);
            if (remoteItem == null) remoteRev.Rev = 0;
            if (localItem == null) localRev.Rev = 0;

            if (localRev < remoteRev) return SyncAction.Download;
            if (localRev > remoteRev) return SyncAction.Upload;

            if (remoteItem != null && localItem != null)
            {
                FileInfo localFile = new FileInfo(localItem.ExternalItemId);
                (ulong length, DateTime date) = await disk.GetInfoFile(remoteProject, remoteItem);

                if (!localFile.Exists || (date > localFile.LastWriteTime && length != 0))
                    return SyncAction.Download;
                if (localFile.Exists || date < localFile.LastWriteTime)
                    return SyncAction.Upload;
            }

            return SyncAction.None;
        }

        public async Task DownloadRemote(int id)
        {
            FindRemote(id);
            FindLocal(id);

            var path = PathManager.GetLocalProjectDir(remoteProject);
            try
            {
                await disk.DownloadItemFile(remoteItem, path);
                if (localItem == null)
                {
                    localItems.Add(remoteItem);
                }
                else
                {
                    int index = localItems.FindIndex(x => x.ID == remoteItem.ID);
                    localItems[index] = remoteItem;
                }
            }
            catch (FileNotFoundException)
            { // Файл удален с сервера но отметки об удалении нет
                // TODO: Тут надо предлжить действие на выбор, или удалить локальный файл если он есть или загрузить локальный
                // Пока по умолчанию востанавливаем отсутвующие файлы
                if (localItem != null && localItem.ExternalItemId != null)
                {
                    FileInfo localFile = new FileInfo(localItem.ExternalItemId);
                    if (localFile.Exists)
                    {
                        await disk.UnloadFileItem(remoteProject, localItem);
                    }
                }
            }
        }

        public async Task UploadLocal(int id)
        {
            FindRemote(id);
            FindLocal(id);
            try
            {
                await disk.UnloadFileItem(remoteProject, localItem);
                if (remoteItem == null)
                {
                    remoteItems.Add(localItem);
                }
                else
                {
                    int index = remoteItems.FindIndex(x => x.ID == remoteItem.ID);
                    remoteItems[index] = localItem;
                }
            }
            catch (FileNotFoundException)
            {// Файл удален локально, но отметки об удалении нет
                // TODO: Тут надо предлжить действие на выбор, или удалить локальный файл если он есть или загрузить локальный
                // Пока по умолчанию востанавливаем отсутвующие файлы
                if (remoteItem != null && remoteItem.ExternalItemId != null)
                {
                    (ulong length, DateTime date) = await disk.GetInfoFile(remoteProject, remoteItem);
                    if (length > 0)
                    {
                        var path = PathManager.GetLocalProjectDir(remoteProject);
                        await disk.DownloadItemFile(remoteItem, path);
                    }
                }
            }
        }

        public Task DeleteLocal(int id)
        {
            var id1 = (ID<ItemDto>)id;
            localItems.RemoveAll(x => x.ID == id1);
            return Task.CompletedTask;
        }

        public Task DeleteRemote(int id)
        {
            var id1 = (ID<ItemDto>)id;
            remoteItems.RemoveAll(x => x.ID == id1);
            return Task.CompletedTask;
        }

        private void FindRemote(int id)
        {
            var id1 = (ID<ItemDto>)id;
            if (remoteItem?.ID != id1)
                remoteItem = remoteItems.Find(x => x.ID == id1);
        }

        private void FindLocal(int id)
        {
            var id1 = (ID<ItemDto>)id;
            if (localItem?.ID != id1)
                localItem = localItems.Find(x => x.ID == id1);
        }
    }
}
