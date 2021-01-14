using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement
{
    public class UserSynchronizer : ISynchronizer
    {
        private List<UserDto> users;
        private YandexDiskManager yandex;
        private UserDto remoteUser;
        private UserDto localUser;

        public UserSynchronizer(YandexDiskManager yandex)
        {
            this.yandex = yandex;
        }


        public List<Revision> GetRevision(Revisions revisions)
        {
            if (revisions.Users == null)
                revisions.Users = new List<Revision>();
            return revisions.Users;
        }
        public void SetRevision(Revisions revisions, List<Revision> userRevs)
        {
            foreach (var rev in userRevs)
            {
                var index = revisions.Users.FindIndex(x => x.ID == rev.ID);
                if (index < 0)
                    revisions.Users.Add(new Revision(rev.ID, rev.Rev));
                else
                    revisions.Users[index].Rev = rev.Rev;
            }
        }

        public List<ISynchronizer> GetSubSynchronizes(int id) => null;

        public void LoadLocalCollect() => users = ObjectModel.GetUsers();
        public void SaveLocalCollect() => ObjectModel.SaveUsers(users);

        public async Task<bool> RemoteExistAsync(int id)
        {
            remoteUser = await yandex.GetUserAsync((ID<UserDto>)id);
            return remoteUser != null;
        }
        public async Task DownloadAndUpdateAsync(int id)
        {
            if ((int)remoteUser.ID != id)
                remoteUser = await yandex.GetUserAsync((ID<UserDto>)id);
            var index = users.FindIndex(x => (int)x.ID == id);
            if (index < 0) 
                users.Add(remoteUser);
            else 
                users[index] = remoteUser;
        }
        public void DeleteLocal(int id) => users.RemoveAll(x => (int)x.ID == id);


        public bool LocalExist(int id)
        {
            localUser = users.Find(x => (int)x.ID == id);
            return localUser != null;
        }
        public async Task UpdateRemoteAsync(int id)
        {
            if ((int)localUser.ID != id)
                localUser = users.Find(x => (int)x.ID == id);
            await yandex.UnloadUser(localUser);
        }
        public async Task DeleteRemoteAsync(int id) => await yandex.DeleteUser((ID<UserDto>)id);
    }
}