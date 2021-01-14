using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MRS.DocumentManagement
{
    internal class ItemsSynchronizer : ISynchronizer
    {
        private YandexDiskManager yandex;
        private ProjectDto project;
        private List<ItemDto> items;
        private ItemDto remoteItem;
        private ItemDto localItem;
        private ObjectiveDto objective = null;

        public ItemsSynchronizer(YandexDiskManager yandex, ProjectDto localProject)
        {
            this.yandex = yandex;
            this.project = localProject;
        }

        public ItemsSynchronizer(YandexDiskManager yandex, ProjectDto localProject, ObjectiveDto objective) : this(yandex, localProject)
        {
            this.objective = objective;
        }

        public List<Revision> GetRevision(Revisions revisions)
        {
            var idPro = (int)project.ID;
            var projectRev = revisions.Projects.Find(pr => pr.ID == idPro);
            if (projectRev == null) 
                return new List<Revision>();
            if (objective == null)
            {
                if (projectRev.Items == null)
                    projectRev.Items = new List<Revision>();
                return projectRev.Items;
            }
            else 
            {
                var idObj = (int)objective.ID;
                var objectiveRev = projectRev.Objectives.Find(o => o.ID == idObj);
                return objectiveRev.Items;
            }
        }

        public void SetRevision(Revisions revisions, List<Revision> itemsRevs)
        {
            int idProj = (int)project.ID;
            var projectRev = revisions.Projects.Find(x => x.ID == idProj);
            if (objective == null)
            {
                CopyRevision(itemsRevs, projectRev);
            }
            else
            {
                var idObj = (int)objective.ID;
                var objectiveRev = projectRev.Objectives.Find(o => o.ID == idObj);

                CopyRevision(itemsRevs, objectiveRev);                
            }
        }

        private static void CopyRevision(List<Revision> itemsRevs, ObjectiveRevision revision)
        {
            if (revision.Items == null)
                revision.Items = new List<Revision>();
            foreach (var rev in itemsRevs)
            {
                var index = revision.Items.FindIndex(x => x.ID == rev.ID);
                if (index < 0)
                    revision.Items.Add(new Revision(rev.ID, rev.Rev));
                else
                    revision.Items[index].Rev = rev.Rev;
            }
        }

        public List<ISynchronizer> GetSubSynchronizes(int id) => null;

        public void LoadLocalCollect()
        {
            if (objective == null)
                items = ObjectModel.GetItems(project);
            else
                items = ObjectModel.GetItems(project, objective);
        }

        public void SaveLocalCollect()
        {
            if (objective == null)
                ObjectModel.SaveItems(project, items);
            else
                ObjectModel.SaveItems(project, objective, items);

        }

        public async Task<bool> RemoteExistAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            await GetItem(_id);

            return remoteItem != null;
        }

        private async Task GetItem(ID<ItemDto> _id)
        {
            if (objective == null)
                remoteItem = await yandex.GetItemAsync(project, _id);
            else
                remoteItem = await yandex.GetItemAsync(project, objective.ID, _id);
        }

        public async Task DownloadAndUpdateAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            if (remoteItem.ID != _id)
                await GetItem(_id);
            int index = items.FindIndex(x => x.ID == _id);
            if (index < 0)
            {
                items.Add(remoteItem);
                //
                // TODO: Раскидывать файлы по папочкам здесь!!! 
                string path = PathManager.GetProjectDir(project);
                //
                await yandex.DownloadItem(remoteItem, path);
                
            }
            else
            {
                // TODO: Удалить старый файл?                            
                items[index] = remoteItem;
            }
        }
        public void DeleteLocal(int id)
        {
            var _id = (ID<ItemDto>)id;
            items.RemoveAll(x => x.ID == _id);
        }

        public bool LocalExist(int id)
        {
            var _id = (ID<ItemDto>)id;
            localItem = items.Find(x => x.ID == _id);
            return localItem != null;
        }
        public async Task UpdateRemoteAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            if (localItem.ID != _id)
                localItem = items.Find(x => x.ID == _id);
            // TODO: Тотже вопрос, если есть старый файл куда его?
            if (objective == null)
                await yandex.UploadItemAsync(project, localItem);
            else
                await yandex.UploadItemAsync(project, objective,  localItem);

        }
        public async Task DeleteRemoteAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            // TODO: Удалять файлы? Сначало понять ссылаются ли другие item на него 
            if (objective == null)
                await yandex.DeleteItem(project, _id);
            else
                await yandex.DeleteItem(project, objective, _id);
        }

    }
}