using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Collections.Generic;
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

        public async Task DeleteLocal(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                context.Items.Remove(local);
            }
        }


        public async Task DeleteRemote(SyncAction action)
        {
            await disk.Delete<ItemDto>(action.ID.ToString());
        }

        public async Task Download(SyncAction action)
        {
            await GetRemote(action.ID);
            await GetLocal(action.ID);
            if (remote != null)
            {
                if (local == null)
                {
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
                await disk.Push(Convert(local), action.ID.ToString());
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