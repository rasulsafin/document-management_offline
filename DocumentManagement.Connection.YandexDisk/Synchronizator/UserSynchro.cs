using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class UserSynchro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private User local;
        private UserSync remote;

        public UserSynchro(IDiskManager disk, DMContext context)
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
            if (local != null)
            {
                remote.Update(local);
            }
            else
            {
                local = new User()
                {
                    ID = remote.ID,
                    Login = remote.Login,
                    Name = remote.Name,
                };
                context.Users.Add(local);
            }
        }

        public Task Special(SyncAction action)
        {
            throw new NotImplementedException();
        }

        public async Task Upload(SyncAction action)
        {
            remote = new UserSync(await GetLocal(action.ID));
            await disk.Push(remote, action.ID.ToString());
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.GetRevisions(TableRevision.Users);
        }

        public Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action)
        {
            return Task.FromResult<List<ISynchroTable>>(null);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetRevision(TableRevision.Users, rev.ID).Rev = rev.Rev;
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

        private async Task<UserSync> GetRemote(int id)
        {
            if (remote?.ID != id)
            {
                remote = await disk.Pull<UserSync>(id.ToString());
            }

            return remote;
        }

        public void CheckDBRevision(RevisionCollection local)
        {
            var allId = context.Users.Select(x => x.ID).ToList();

            var revCollect = local.GetRevisions(TableRevision.Users);
            foreach (var id in allId)
            {
                if (!revCollect.Any(x => x.ID == id))
                {
                    revCollect.Add(new Revision(id));
                }
            }
        }

        public class UserSync
        {
            public UserSync()
            { }

            public UserSync(User local)
            {
                ID = local.ID;
                Login = local.Login;
                Name = local.Name;
            }

            public int ID { get; set; }

            public string Login { get; set; }

            public string Name { get; set; }

            public UserDto ToDto()
            {
                return new UserDto(
                    id: new ID<UserDto>(ID),
                    login: Login,
                    name: Name);
            }

            internal void Update(User local)
            {
                local.Login = Login;
                local.Name = Name;
            }
        }
    }
}
