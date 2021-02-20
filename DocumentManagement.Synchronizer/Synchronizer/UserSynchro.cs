using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Synchronizer
{
    public class UserSynchro : ISynchroTable
    {
        private ICloudManager disk;
        private DMContext context;
        private User local;
        private UserSync remote;

        public UserSynchro(ICloudManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        public async Task DeleteLocal(SyncAction action)
        {
            local = await GetLocal(action.ID);
            context.Users.Remove(local);
            context.SaveChanges();
            action.IsComplete = true;
        }

        public async Task DeleteRemote(SyncAction action)
        {
            await disk.Delete<UserSync>(action.ID.ToString());
            action.IsComplete = true;
        }

        public async Task Download(SyncAction action)
        {
            remote = await GetRemote(action.ID);
            local = await GetLocal(action.ID);
            action.IsComplete = true;

            if (remote != null)
            {
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
            else
            {
                action.IsComplete = false;
            }
        }

        public async Task Upload(SyncAction action)
        {
            var user = await GetLocal(action.ID);
            if (user != null)
            {
                remote = new UserSync();
                await disk.Push(remote, action.ID.ToString());
                action.IsComplete = true;
            }
            else
            {
                action.IsComplete = false;
            }
            remote = new UserSync(await GetLocal(action.ID));
            await disk.Push(remote, action.ID.ToString());
            action.IsComplete = true;
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.GetRevisions(NameTypeRevision.Users);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetRevision(NameTypeRevision.Users, rev.ID).Rev = rev.Rev;
        }

        public void CheckDBRevision(RevisionCollection local)
        {
            var allId = context.Users.Select(x => x.ID).ToList();

            var revCollect = local.GetRevisions(NameTypeRevision.Users);
            foreach (var id in allId)
            {
                if (!revCollect.Any(x => x.ID == id))
                {
                    revCollect.Add(new Revision(id));
                }
            }
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
            // if (remote?.ID != id)
            // {
            // remote = await disk.Pull<UserSync>(id.ToString());
            //  }

            return remote;
        }

        public class UserSync
        {
            public UserSync()
            {
            }

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
