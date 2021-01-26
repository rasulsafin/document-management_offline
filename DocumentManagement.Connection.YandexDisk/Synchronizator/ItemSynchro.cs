using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
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

        public Task Download(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.Items;
        }

        public Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action)
        {
            return Task.FromResult<List<ISynchroTable>>(null);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetItem(rev.ID).Rev = rev.Rev;
        }

        public Task Special(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public SyncAction SpecialSynchronization(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public Task Upload(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        private async Task GetLocal(int id)
        {
            if (local?.ID == id)
                local = await context.Items.FindAsync(id);
        }

        private async Task GetRemote(int id)
        {
            if (remote?.ID == new ID<ItemDto>(id))
                remote = await disk.Pull<ItemDto>(id.ToString());
        }
    }

    internal class ItemSync
    {
    }
}