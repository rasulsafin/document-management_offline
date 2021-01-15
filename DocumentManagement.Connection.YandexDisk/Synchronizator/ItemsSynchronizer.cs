using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    internal class ItemsSynchronizer : ISynchronizer
    {
        private DiskManager disk;
        private DMContext context;
        private ProjectDto project;
        private ObjectiveDto objective;
        private ItemDto remoteItem;
        private Item localItem;

        public ItemsSynchronizer(DiskManager disk, DMContext context, ProjectDto project) : this(disk, context, project, null){}

        public ItemsSynchronizer(DiskManager disk, DMContext context,  ProjectDto project, ObjectiveDto objectiveDto)
        {
            this.disk = disk;
            this.project = project;
            this.objective = objectiveDto;
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
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
                if (objectiveRev == null)
                    return new List<Revision>();
                if (objectiveRev.Items == null)
                    objectiveRev.Items = new List<Revision>();
                return objectiveRev.Items;
            }
        }
        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            int idProj = (int)project.ID;
            var projectRev = revisions.Projects.Find(x => x.ID == idProj);
            if (objective == null)
            {
                CopyRevision(rev, projectRev);
            }
            else
            {
                var idObj = (int)objective.ID;
                var objectiveRev = projectRev.Objectives.Find(o => o.ID == idObj);

                if (objectiveRev != null)
                {
                    CopyRevision(rev, objectiveRev);
                }
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
            //if (objective == null)
            //    items = ObjectModel.GetItems(project);
            //else
            //    items = ObjectModel.GetItems(project, objective);
        }

        public async Task SaveLocalCollectAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task<bool> RemoteExist(int id)
        {
            
            await Download(id);
            return remoteItem != null;
        }
        private async Task Download(int id)
        {
            var _id = (ID<ItemDto>)id;
            if (objective == null)
                remoteItem = await disk.GetItemAsync(project, _id);
            else
                remoteItem = await disk.GetItemAsync(project, objective.ID, _id);
        }

        public async Task DownloadAndUpdateAsync(int id)
        {
            await Download(id);
            if (await LocalExist(id))
            {                
                // TODO: Раскидывать файлы по папочкам здесь!!! 
                // TODO: Удалить старый файл? 
                string path = PathManager.GetProjectDir(project);                
                await disk.DownloadItem(remoteItem, path);

                
            }
            else
            {
                string path = PathManager.GetProjectDir(project);
                await disk.DownloadItem(remoteItem, path);
                context.Items.Add(Convert(remoteItem));
            }
        }
        public async Task DeleteLocalAsync(int id)
        {
            if (await LocalExist(id))
                context.Items.Remove(localItem);
        }


        public async Task<bool> LocalExist(int id)
        {
            await Find(id);
            return localItem != null;
        }

        private async Task Find(int id)
        {
            if (localItem?.ID != id)
                localItem =await context.Items.FindAsync(id);
        }
        public async Task UpdateRemoteAsync(int id)
        {
            await Find(id);            
            if (objective == null)
                await disk.UploadItemAsync(project, Convert(localItem));
            else
                await disk.UploadItemAsync(project, objective, Convert(localItem));
        }


        public async Task DeleteRemoteAsync(int id)
        {
            var _id = (ID<ItemDto>)id;
            // TODO: Удалять файлы? Сначало понять ссылаются ли другие item на него 
            if (objective == null)
                await disk.DeleteItem(project, _id);
            else
                await disk.DeleteItem(project, objective, _id);
        }

        private ItemDto Convert(Item model)
        {
            return new ItemDto()
            { 
                ID = new ID<ItemDto>(model.ID)
                ,Name = model.Name
                ,ExternalItemId= model.ExternalItemId
                ,ItemType= (ItemTypeDto)model.ItemType                
            };
        }
        private Item Convert(ItemDto dto)
        {
            return new Item()
            {
                ID = (int)dto.ID
                ,Name = dto.Name
                ,ExternalItemId = dto.ExternalItemId
                ,ItemType = (int)dto.ItemType
            };
        }
    }
}