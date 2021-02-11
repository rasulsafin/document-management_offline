using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.Synchronizer
{
    public class ItemSynchro : ISynchroTable
    {
        private ICloudManager disk;
        private DMContext context;
        private Item local;
        private ItemDto remote;

        public ItemSynchro(ICloudManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        public void CheckDBRevision(RevisionCollection local)
        {
            var allId = context.Items.Select(x => x.ID).ToList();

            var revCollect = local.GetRevisions(NameTypeRevision.Items);
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
                action.IsComplete = true;
                context.Items.Remove(local);
            }
        }

        public async Task DeleteRemote(SyncAction action)
        {
            await GetRemote(action.ID);
            if (remote != null)
            {
                action.IsComplete = true;
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
                    context.Items.Add(Convert(remote));
                }
                else
                {
                    local.ItemType = (int)remote.ItemType;
                    local.Name = remote.Name;
                    local.ExternalItemId = remote.ExternalItemId;
                }

                context.SaveChanges();
                action.IsComplete = true;
            }
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.GetRevisions(NameTypeRevision.Items);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetRevision(NameTypeRevision.Items, rev.ID).Rev = rev.Rev;
        }

        public async Task Upload(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                remote = Convert(local);
                await disk.Push(remote, action.ID.ToString());
                action.IsComplete = true;
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
