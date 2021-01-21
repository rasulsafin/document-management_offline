using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement
{
    public class UserSynchronizer : ISynchronizer
    {
        private List<UserDto> users;
        private DiskManager disk;
        private UserDto remoteUser;
        private UserDto localUser;
        private List<UserDto> remoteUsers;

        public UserSynchronizer(DiskManager disk)
        {
            this.disk = disk;
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            if (revisions.Users == null)
                return new List<Revision>();
            return new List<Revision>(revisions.Users);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            if (revisions.Users == null)
                revisions.Users = new List<Revision>();
            var index = revisions.Users.FindIndex(x => x.ID == rev.ID);
            if (index < 0)
                revisions.Users.Add(new Revision(rev.ID, rev.Rev));
            else
                revisions.Users[index].Rev = rev.Rev;
        }

        public void LoadCollection() => users = ObjectModel.GetUsers();

        public Task SaveLocalCollectAsync()
        {
            ObjectModel.SaveUsers(users);
            return Task.CompletedTask;
        }

        public Task<List<ISynchronizer>> GetSubSynchronizesAsync(int id) => Task.FromResult<List<ISynchronizer>>(null);

        public async Task<SyncAction> GetActoin(Revision localRev, Revision remoteRev)
        {
            if (localRev == null) localRev = new Revision(remoteRev.ID);
            if (remoteRev == null) remoteRev = new Revision(localRev.ID);
            if (localRev.IsDelete || remoteRev.IsDelete) return SyncAction.Delete;
            await Download(localRev.ID);
            FindLocal(localRev.ID);
            if (remoteUser == null) remoteRev.Rev = 0;
            if (localUser == null) localRev.Rev = 0;

            if (localRev < remoteRev) return SyncAction.Download;
            if (localRev > remoteRev) return SyncAction.Upload;
            return SyncAction.None;
        }

        public async Task DownloadRemote(int id)
        {
            await Download(id);
            var index = users.FindIndex(x => (int)x.ID == id);
            if (index < 0)
                users.Add(remoteUser);
            else
                users[index] = remoteUser;
        }

        public async Task UploadLocal(int id)
        {
            FindLocal(id);
            await disk.UnloadUser(localUser);
        }

        public Task DeleteLocal(int id)
        {
            users.RemoveAll(x => (int)x.ID == id);
            return Task.CompletedTask;
        }


        public async Task DeleteRemote(int id) => await disk.DeleteUser((ID<UserDto>)id);

        private async Task Download(int id)
        {
            var id1 = new ID<UserDto>(id);
            if (remoteUser?.ID != id1)
                remoteUser = await disk.GetUserAsync(id1);
        }

        private void FindLocal(int id)
        {
            var id1 = new ID<UserDto>(id);
            if (localUser?.ID != id1)
                localUser = users.Find(x => x.ID == id1);
        }
    }
}
