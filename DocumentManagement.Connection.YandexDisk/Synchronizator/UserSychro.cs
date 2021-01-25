using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizator
{

    public class UserSychro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private User local;
        private UserSyncModel remote;

        public UserSychro(IDiskManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        public async Task DeleteLocal(SyncAction action)
        {
            local = await GetLocal(action.ID);
            context.Users.Remove(local);
            context.SaveChanges();
        }


        public async Task DeleteRemote(SyncAction action)
        {
            await disk.Delete<UserDto>(action.ID.ToString());
        }

        public async Task Download(SyncAction action)
        {
            remote = await GetRemote(action.ID);
            local = await GetLocal(action.ID);
            remote.Update(local);
        }


        public Task Special(SyncAction action)
        {
            throw new NotImplementedException();
        }

        public async Task Upload(SyncAction action)
        {
            remote = new UserSyncModel(await GetLocal(action.ID));
            disk.Push(remote, action.ID.ToString());
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            if (revisions.Users == null)
                revisions.Users = new List<Revision>();
            return revisions.Users;
        }

        public Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action)
        {
            return Task.FromResult<List<ISynchroTable>>(null);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetUser(rev.ID).Rev = rev.Rev;
        }

        public SyncAction SpecialSynchronization(SyncAction action)
        {
            return action;
        }

        private async Task<User> GetLocal(int id)
        {
            if (local?.ID != id)
            {
                local = await context.Users.FindAsync(id);
            }
            return local;
        }

        private async Task<UserSyncModel> GetRemote(int id)
        {
            if (remote?.ID != id)
            {
                remote = await disk.Pull<UserSyncModel>(id.ToString());
            }

            return remote;
        }

        internal class UserSyncModel
        {
            public UserSyncModel()
            { }

            public UserSyncModel(User local)
            {
                ID = local.ID;
                Login = local.Login;
                Name = local.Name;
                //ID = local.;
            }

            public int ID { get; set; }
            public string Login { get; set; }
            public string Name { get; set; }

            internal void Update(User local)
            {
                local.Login = Login;
                local.Name = Name;
            }
        }
    }

}
