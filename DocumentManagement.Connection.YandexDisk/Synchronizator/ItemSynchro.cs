using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ItemSynchro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private Item local;
        private ItemDto remote;

        public ItemSynchro(IDiskManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        public void CheckDBRevision(RevisionCollection local)
        {
            var allId = context.Items.Select(x => x.ID).ToList();

            var revCollect = local.GetRevisions(TableRevision.Items);
            foreach (var id in allId)
            {
                if (!revCollect.Any(x => x.ID == id))
                {
                    revCollect.Add(new Revision(id));
                }
            }
        }

        public async Task DeleteLocal(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                FileInfo fileInfo = new FileInfo(local.ExternalItemId);
                if (fileInfo.Exists)
                    fileInfo.Delete();

                context.Items.Remove(local);
            }
        }

        public async Task DeleteRemote(SyncAction action)
        {
            await GetRemote(action.ID);
            if (remote != null)
            {
                // Этим будет заниматся отдельный сервер
                // await disk.DeleteFile(remote.ExternalItemId);
                
                await disk.Delete<ItemDto>(action.ID.ToString());
            }

        }

        public async Task Download(SyncAction action)
        {
            await GetRemote(action.ID);
            await GetLocal(action.ID);
            if (remote != null)
            {
                if (local == null)
                {
                    string localDirName = 
                    // На этом компьютере файла ещё нет и нет записи  о нем в базе
                    // Скачать файл, куда?
                    // Как Проект получить?
                    context.Items.Add(Convert(remote));

                }
                else
                {
                    local.ItemType = (int)remote.ItemType;
                    local.Name = remote.Name;
                    local.ExternalItemId = remote.ExternalItemId;
                }

                context.SaveChanges();
            }
        }


        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.GetRevisions(TableRevision.Items);
        }

        public Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action)
        {
            return Task.FromResult<List<ISynchroTable>>(null);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetRevision(TableRevision.Items, rev.ID).Rev = rev.Rev;
        }

        public Task Special(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public SyncAction SpecialSynchronization(SyncAction action)
        {
            return action;
        }

        public async Task Upload(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                remote = Convert(local);
                await disk.Push(remote, action.ID.ToString());

                // Скачиванием файлов будет заниматся отдельный сервис
                //string remoteDirName = string.Empty;
                //ProjectItem projectItem = context.ProjectItems.First(x => x.ItemID == action.ID);
                //if (projectItem != null)
                //{
                //    Project project = projectItem.Project;
                //    remoteDirName = project.Title;
                //}
                //else
                //{
                //    ObjectiveItem objectiveItem = context.ObjectiveItems.First(x => x.ItemID == action.ID);
                //    if (objectiveItem != null)
                //    {
                //        Project project = objectiveItem.Objective.Project;
                //        remoteDirName = project.Title;
                //    }
                //}
                //FileInfo info = new FileInfo(local.ExternalItemId);
                //if (remoteDirName != string.Empty && info.Exists)
                //{
                //    var localDirName = info.DirectoryName;
                //    await disk.PushFile(remoteDirName, localDirName, info.Name);
                //    
                //    // Вставляем путь к файлу в системе
                //    remote.ExternalItemId = PathManager.GetFile(remoteDirName, info.Name);
                //}
                // else // TODO: Если файла нет можно удалять его из системы???
            }
        }

        private ItemDto Convert(Item item)
        {
            return new ItemDto()
            {
                ID = (ID<ItemDto>)item.ID,
                ItemType = (ItemTypeDto)item.ItemType,
                Name = item.Name,
                ExternalItemId = item.ExternalItemId,
            };
        }

        private Item Convert(ItemDto item)
        {
            return new Item()
            {
                ID = (int)item.ID,
                ItemType = (int)item.ItemType,
                Name = item.Name,
                ExternalItemId = item.ExternalItemId,
            };
        }

        private async Task GetLocal(int id)
        {
            if (local?.ID != id)
                local = await context.Items.FindAsync(id);
        }

        private async Task GetRemote(int id)
        {
            if (remote?.ID != new ID<ItemDto>(id))
                remote = await disk.Pull<ItemDto>(id.ToString());
        }
    }


}