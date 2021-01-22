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
    // public class ItemsSynchronizer : ISynchronizer
    // {
    //    private DiskManager disk;
    //    private ProjectDto remoteProject;
    //    private ProjectDto localProject;
    //    private ObjectiveDto remoteObj;
    //    private ObjectiveDto localObj;
    //    private List<ItemDto> remoteItems;
    //    private List<ItemDto> localItems;
    //    private ItemDto remoteItem;
    //    private ItemDto localItem;
    //    private bool toObjective;
    //    private long localLength;
    //    private long remoteLength;
    //    private DateTime localDate;
    //    private string localPath;
    //    private DateTime remoteDate;
    //    private string remotePath;
    //    private int infoId;

    // public ItemsSynchronizer(DiskManager yandex, ProjectDto remoteProject, ProjectDto localProject)
    //    {
    //        this.disk = yandex;
    //        this.remoteProject = remoteProject;
    //        this.localProject = localProject;
    //        toObjective = false;
    //        if (remoteProject == null) throw new NullReferenceException("remoteProject is null");
    //        if (localProject == null) throw new NullReferenceException("localProject is null");
    //    }

    // public ItemsSynchronizer(DiskManager yandex, ProjectDto actualProject, ObjectiveDto remoteObj, ObjectiveDto localObj)
    //    {
    //        this.disk = yandex;
    //        this.remoteProject = actualProject;
    //        this.localProject = actualProject;
    //        this.remoteObj = remoteObj;
    //        this.localObj = localObj;
    //        if (remoteObj == null) throw new NullReferenceException("remoteObj is null");
    //        if (localObj == null) throw new NullReferenceException("localObj is null");
    //        toObjective = true;
    //    }

    // public string NameElement { get; set; }

    // public List<Revision> GetRevisions(RevisionCollection revisions)
    //    {
    //        var idPro = (int)remoteProject.ID;
    //        var projectRev = revisions.Projects.Find(pr => pr.ID == idPro);
    //        if (projectRev == null)
    //            return new List<Revision>();

    // if (!toObjective)
    //        {
    //            if (projectRev.Items == null)
    //                projectRev.Items = new List<Revision>();
    //            return projectRev.Items;
    //        }
    //        else
    //        {
    //            int idObj = (int)remoteObj.ID;
    //            if (projectRev.Objectives == null)
    //                projectRev.Objectives = new List<ObjectiveRevision>();
    //            var objective = projectRev.Objectives.Find(x => x.ID == idObj);
    //            if (objective == null)
    //                return new List<Revision>();
    //            if (objective.Items == null)
    //                objective.Items = new List<Revision>();
    //            return objective.Items;
    //        }
    //    }

    // public void SetRevision(RevisionCollection revisions, Revision rev)
    //    {
    //        int idProj = (int)remoteProject.ID;
    //        var projectRev = revisions.GetProject(idProj);

    // if (!toObjective)
    //        {
    //            CopyRevision(rev, projectRev);
    //        }
    //        else
    //        {
    //            int idObj = (int)remoteObj.ID;
    //            var objective = projectRev.FindObjetive(idObj);
    //            CopyRevision(rev, objective);
    //        }

    // void CopyRevision(Revision rev, ObjectiveRevision revision)
    //        {
    //            var editRev = revision.FindItem(rev.ID);
    //            editRev.Rev = rev.Rev;
    //        }
    //    }

    // public Task<List<ISynchronizer>> GetSubSynchronizesAsync(int id)
    //    {
    //        return Task.FromResult<List<ISynchronizer>>(null);
    //    }

    // public Task LoadCollection()
    //    {
    //        if (toObjective)
    //        {
    //            remoteItems = remoteObj?.Items?.ToList();
    //            localItems = localObj?.Items?.ToList();
    //        }
    //        else
    //        {
    //            remoteItems = remoteProject?.Items?.ToList();
    //            localItems = localProject?.Items?.ToList();
    //        }

    // if (remoteItems == null) remoteItems = new List<ItemDto>();
    //        if (localItems == null) localItems = new List<ItemDto>();
    //        return Task.CompletedTask;
    //    }

    // public async Task SaveCollectionAsync()
    //    {
    //        if (toObjective)
    //        {
    //            localObj.Items = localItems;
    //            remoteObj.Items = remoteItems;
    //            await disk.UploadObjectiveAsync(localProject, remoteObj);
    //        }
    //        else
    //        {
    //            localProject.Items = localItems;
    //            remoteProject.Items = remoteItems;
    //            await disk.UnloadProject(localProject);
    //        }
    //    }

    // public async Task<SyncAction> GetActoin(Revision localRev, Revision remoteRev)
    //    {
    //        if (localRev == null) localRev = new Revision(remoteRev.ID);
    //        if (remoteRev == null) remoteRev = new Revision(localRev.ID);
    //        FindLocal(localRev.ID);
    //        if (toObjective)
    //            NameElement = $"proj({localProject.ID}) obj({localObj.ID}) item({localItem?.ExternalItemId})";
    //        else
    //            NameElement = $"proj({localProject.ID}) item({localItem?.ExternalItemId})";
    //        if (localRev.IsDelete || remoteRev.IsDelete) return SyncAction.Delete;

    // FindRemote(localRev.ID);
    //        if (remoteItem == null) remoteRev.Rev = 0;
    //        if (localItem == null) localRev.Rev = 0;

    // if (localRev < remoteRev) return SyncAction.Download;
    //        if (localRev > remoteRev) return SyncAction.Upload;

    // if (remoteItem != null && localItem != null)
    //        {
    //            await GetInfoOfFiles(localRev.ID);

    // if (remoteLength > 0 && (localDate < remoteDate))
    //                return SyncAction.Download;
    //            if (localLength > 0 && (localDate > remoteDate))
    //                return SyncAction.Upload;
    //        }

    // return SyncAction.None;
    //    }

    // public async Task DownloadRemote(int id)
    //    {
    //        await GetInfoOfFiles(id);
    //        try
    //        {
    //            var path = PathManager.GetLocalProjectDir(remoteProject);
    //            await disk.DownloadItemFile(remoteItem, path);
    //            if (localItem == null)
    //            {
    //                localItems.Add(remoteItem);
    //            }
    //            else
    //            {
    //                int index = localItems.FindIndex(x => x.ID == remoteItem.ID);
    //                localItems[index] = remoteItem;
    //            }
    //        }
    //        catch (FileNotFoundException)
    //        { // Файл удален с сервера но отметки об удалении нет
    //            // TODO: Тут надо предлжить действие на выбор, или удалить локальный файл если он есть или загрузить локальный
    //            // Пока по умолчанию востанавливаем отсутвующие файлы
    //            if (localLength > 0)
    //            {
    //                FileInfo localFile = new FileInfo(localPath);
    //                if (localFile.Exists)
    //                {
    //                    remoteItem.ExternalItemId = localPath;
    //                    await disk.UnloadFileItem(remoteProject, remoteItem);
    //                }
    //            }
    //        }
    //    }

    // public async Task UploadLocal(int id)
    //    {
    //        await GetInfoOfFiles(id);
    //        try
    //        {
    //            await disk.UnloadFileItem(remoteProject, localItem);
    //            if (remoteItem == null)
    //            {
    //                remoteItems.Add(localItem);
    //            }
    //            else
    //            {
    //                int index = remoteItems.FindIndex(x => x.ID == remoteItem.ID);
    //                remoteItems[index] = localItem;
    //            }
    //        }
    //        catch (FileNotFoundException)
    //        {// Файл удален локально, но отметки об удалении нет
    //            // TODO: Тут надо предлжить действие на выбор, или удалить локальный файл если он есть или загрузить локальный
    //            // Пока по умолчанию востанавливаем отсутвующие файлы
    //            if (remoteLength > 0)
    //            {
    //                var path = PathManager.GetLocalProjectDir(remoteProject);
    //                localItem.ExternalItemId = remotePath;
    //                await disk.DownloadItemFile(localItem, path);
    //            }
    //        }
    //    }

    // public Task DeleteLocal(int id)
    //    {
    //        var id1 = (ID<ItemDto>)id;
    //        localItems.RemoveAll(x => x.ID == id1);
    //        return Task.CompletedTask;
    //    }

    // public Task DeleteRemote(int id)
    //    {
    //        var id1 = (ID<ItemDto>)id;
    //        remoteItems.RemoveAll(x => x.ID == id1);
    //        return Task.CompletedTask;
    //    }

    // private async Task GetInfoOfFiles(int id)
    //    {
    //        if (infoId == id) return;
    //        infoId = id;
    //        FindRemote(id);
    //        FindLocal(id);

    // localLength = -1;
    //        localDate = DateTime.MinValue;
    //        localPath = string.Empty;
    //        if (localItem != null)
    //        {
    //            FileInfo localFile = new FileInfo(localItem.ExternalItemId);
    //            if (localFile.Exists)
    //            {
    //                localLength = localFile.Length;
    //                localDate = localFile.LastWriteTime;
    //                localPath = localFile.FullName;
    //            }
    //        }

    // remoteLength = -1;
    //        remoteDate = DateTime.MinValue;
    //        remotePath = string.Empty;
    //        if (remoteItem != null)
    //        {
    //            (long length, DateTime date, string path) = await disk.GetInfoFile(remoteProject, remoteItem);
    //            remoteLength = length;
    //            remoteDate = date;
    //            remotePath = path;
    //        }
    //    }

    // private void FindRemote(int id)
    //    {
    //        var id1 = (ID<ItemDto>)id;
    //        if (remoteItem?.ID != id1)
    //            remoteItem = remoteItems.Find(x => x.ID == id1);
    //    }

    // private void FindLocal(int id)
    //    {
    //        var id1 = (ID<ItemDto>)id;
    //        if (localItem?.ID != id1)
    //            localItem = localItems.Find(x => x.ID == id1);
    //    }
    // }
}
