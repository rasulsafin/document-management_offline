using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
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

        public ItemsSynchronizer(YandexDiskManager yandex, ProjectDto localProject)
        {
            this.yandex = yandex;
            this.project = localProject;
        }

        public List<Revision> GetRevision(Revisions revisions)
        {
            var idPro = (int)project.ID;
            var projectRev = revisions.Projects.Find(pr => pr.ID == idPro);
            if (projectRev.Items == null)
                projectRev.Items = new List<Revision>();
            return projectRev.Items;
        }

        public List<ISynchronizer> GetSubSynchronizes(int id) => null;

        public void LoadLocalCollect()
        {
            items = ObjectModel.GetItems(project);
        }

        public void SaveLocalCollect()
        {
            ObjectModel.SaveItems(project, items);
        }

        public async Task<bool> RemoteExistAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            remoteItem = await yandex.GetItemAsync(project, _id);
            return remoteItem == null;
        }
        public async Task DownloadAndUpdateAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            if (remoteItem.ID == _id) 
                remoteItem = await yandex.GetItemAsync(project, _id);
            int index = items.FindIndex(x => x.ID == _id);
            if (index < 0)
            {
                items.Add(remoteItem);
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
        }
        public async Task UpdateRemoteAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            if (localItem.ID == _id)
                localItem = items.Find(x => x.ID == _id);
            // TODO: Тотже вопрос, если есть старый файл куда его?
            await yandex.UploadItemAsync(project, localItem);
        }
        public async Task DeleteRemoteAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            // TODO: Удалять файлы? Сначало понять ссылаются ли другие item на него 
            await yandex.DeleteItem(project, _id);
        }

    }
}