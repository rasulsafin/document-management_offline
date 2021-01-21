using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    // internal class ItemsSynchronizer : ISynchronizer
    // {
    //    private DiskManager disk;
    //    private DMContext context;
    //    private ProjectDto project;
    //    private ObjectiveDto objective;
    //    private ItemDto remoteItem;
    //    private Item localItem;

    // public ItemsSynchronizer(DiskManager disk, DMContext context, ProjectDto project) : this(disk, context, project, null) { }

    // public ItemsSynchronizer(DiskManager disk, DMContext context, ProjectDto project, ObjectiveDto objectiveDto)
    //    {
    //        this.disk = disk;
    //        this.project = project;
    //        this.objective = objectiveDto;
    //    }
    //    public string NameElement { get; set; }

    // public List<Revision> GetRevisions(RevisionCollection revisions)
    //    {
    //        var idPro = (int)project.ID;
    //        var projectRev = revisions.Projects.Find(pr => pr.ID == idPro);
    //        if (projectRev == null)
    //            return new List<Revision>();
    //        if (objective == null)
    //        {
    //            if (projectRev.Items == null)
    //                projectRev.Items = new List<Revision>();
    //            return projectRev.Items;
    //        }
    //        else
    //        {
    //            var idObj = (int)objective.ID;
    //            var objectiveRev = projectRev.Objectives.Find(o => o.ID == idObj);
    //            if (objectiveRev == null)
    //                return new List<Revision>();
    //            if (objectiveRev.Items == null)
    //                objectiveRev.Items = new List<Revision>();
    //            return objectiveRev.Items;
    //        }
    //    }

    // public void SetRevision(RevisionCollection revisions, Revision rev)
    //    {
    //        int idProj = (int)project.ID;
    //        var projectRev = revisions.Projects.Find(x => x.ID == idProj);
    //        if (objective == null)
    //        {
    //            CopyRevision(rev, projectRev);
    //        }
    //        else
    //        {
    //            var idObj = (int)objective.ID;
    //            var objectiveRev = projectRev.Objectives.Find(o => o.ID == idObj);

    // if (objectiveRev != null)
    //            {
    //                CopyRevision(rev, objectiveRev);
    //            }
    //        }

    // void CopyRevision(Revision rev, ObjectiveRevision revision)
    //        {
    //            if (revision.Items == null)
    //                revision.Items = new List<Revision>();

    // var index = revision.Items.FindIndex(x => x.ID == rev.ID);
    //            if (index < 0)
    //                revision.Items.Add(new Revision(rev.ID, rev.Rev));
    //            else
    //                revision.Items[index].Rev = rev.Rev;
    //        }
    //    }

    // public Task<List<ISynchronizer>> GetSubSynchronizesAsync(int id) => null;

    // public void LoadCollection()
    //    {
    //        // if (objective == null)
    //        //    items = ObjectModel.GetItems(project);
    //        // else
    //        //    items = ObjectModel.GetItems(project, objective);
    //    }

    // public async Task SaveLocalCollectAsync()
    //    {
    //        await context.SaveChangesAsync();
    //    }

    // public Task<bool> RemoteExist(int id)
    //    {
    //        return Task.FromResult(true);
    //    }

    // public async Task DownloadRemote(int id)
    //    {
    //        ////
    //        //// TODO : Проверить есть ли такой файл уже
    //        ////
    //        //// await FindRemote(id);
    //        // var item = Convert(remoteItem);
    //        // bool needDownload = false;

    // // (ulong contentLength, DateTime remoteLastWrile) =  await disk.GetInfoFile(project, remoteItem);
    //        // FileInfo local = new FileInfo(item.ExternalItemId);

    // // needDownload = local.LastWriteTime < remoteLastWrile && contentLength != 0;

    // // if (needDownload)
    //        // {
    //        //    string path = PathManager.GetLocalProjectDir(project);
    //        //    await disk.DownloadItemFile(remoteItem, path);
    //        // }
    //    }



    // public async Task DeleteLocal(int id)
    //    {
    //        if (await LocalExist(id))
    //            context.Items.Remove(localItem);
    //    }

    // public async Task<bool> LocalExist(int id)
    //    {
    //        await Find(id);
    //        return localItem != null;
    //    }

    // private async Task Find(int id)
    //    {
    //        if (localItem?.ID != id)
    //            localItem = await context.Items.FindAsync(id);
    //    }

    // // private Task FindRemote(int id)
    //    // {
    //    //    if (localItem?.ID != id)
    //    //        localItem = await context.Items.FindAsync(id);
    //    // }

    // public async Task UploadLocal(int id)
    //    {
    //        await Find(id);
    //        await disk.UnloadFileItem(project, Convert(localItem));
    //    }

    // public Task DeleteRemote(int id)
    //    {
    //        // var _id = (ID<ItemDto>)id;
    //        //// TODO: Удалять файлы? Сначало понять ссылаются ли другие item на него
    //        // if (objective == null)
    //        //    await disk.DeleteItem(project, _id);
    //        // else
    //        //    await disk.DeleteItem(project, objective, _id);
    //        return Task.CompletedTask;
    //    }

    // private ItemDto Convert(Item model)
    //    {
    //        return new ItemDto()
    //        {
    //            ID = new ID<ItemDto>(model.ID),
    //            Name = model.Name,
    //            ExternalItemId = model.ExternalItemId,
    //            ItemType = (ItemTypeDto)model.ItemType,
    //        };
    //    }

    // private Item Convert(ItemDto dto)
    //    {
    //        return new Item()
    //        {
    //            ID = (int)dto.ID,
    //            Name = dto.Name,
    //            ExternalItemId = dto.ExternalItemId,
    //            ItemType = (int)dto.ItemType,
    //        };
    //    }

    // public Task<SyncAction> GetActoin(Revision localRev, Revision remoteRev)
    //    {
    //        throw new NotImplementedException();
    //    }
    // }
}
