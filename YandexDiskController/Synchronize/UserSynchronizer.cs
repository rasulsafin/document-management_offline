using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement
{
    public class UserSynchronizer : ISynchronizer
    {
        private List<UserDto> users;
        private DiskManager disk;
        private UserDto remoteUser;
        private UserDto localUser;

        public UserSynchronizer(DiskManager disk)
        {
            this.disk = disk;
        }


        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            if (revisions.Users == null)
                revisions.Users = new List<Revision>();
            return revisions.Users;
        }       

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            var index = revisions.Users.FindIndex(x => x.ID == rev.ID);
            if (index < 0)
                revisions.Users.Add(new Revision(rev.ID, rev.Rev));
            else
                revisions.Users[index].Rev = rev.Rev;
        }

        public async Task<List<ISynchronizer>> GetSubSynchronizesAsync(int id) => null;

        public void LoadLocalCollect() => users = ObjectModel.GetUsers();
        public async Task SaveLocalCollectAsync() => ObjectModel.SaveUsers(users);

        public async Task<bool> RemoteExist(int id)
        {
            remoteUser = await disk.GetUserAsync((ID<UserDto>)id);
            return remoteUser != null;
        }
        public async Task DownloadAndUpdateAsync(int id)
        {
            if ((int)remoteUser.ID != id)
                remoteUser = await disk.GetUserAsync((ID<UserDto>)id);
            var index = users.FindIndex(x => (int)x.ID == id);
            if (index < 0) 
                users.Add(remoteUser);
            else 
                users[index] = remoteUser;
        }
        public async Task DeleteLocalAsync(int id) => users.RemoveAll(x => (int)x.ID == id);


        public async Task<bool> LocalExist(int id)
        {
            localUser = users.Find(x => (int)x.ID == id);
            return localUser != null;
        }
        public async Task UpdateRemoteAsync(int id)
        {
            if ((int)localUser.ID != id)
                localUser = users.Find(x => (int)x.ID == id);
            await disk.UnloadUser(localUser);
        }
        public async Task DeleteRemoteAsync(int id) => await disk.DeleteUser((ID<UserDto>)id);

        
    }
}