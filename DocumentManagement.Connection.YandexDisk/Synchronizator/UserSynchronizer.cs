using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    internal class UserSynchronizer : ISynchronizer
    {
        private DiskManager disk;
        private DMContext context;
        // private DbSet<User> users;
        private UserDto remoteUser;
        private User localUser;

        public UserSynchronizer(DiskManager yandex, DMContext context/*, IMapper mapper*/)
        {
            this.disk = yandex;
            this.context = context;
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

        public Task<List<ISynchronizer>> GetSubSynchronizesAsync(int id) => null;


        public void LoadLocalCollect()
        {
            // users = context.Users;
        }

        public async Task SaveLocalCollectAsync()
        {
            await context.SaveChangesAsync();
        }


        public async Task<bool> RemoteExist(int id)
        {
            await Download(id);
            return remoteUser != null;
        }

        private async Task Download(int id)
        {
            if ((int)remoteUser?.ID != id)
                remoteUser = await disk.GetUserAsync((ID<UserDto>)id);
        }

        public async Task DownloadAndUpdateAsync(int id)
        {
            await Download(id);
            if (await LocalExist(id))
            {
                localUser.Login = remoteUser.Login;
                localUser.Name = remoteUser.Name;
            }
            else
            {
                // TODO: Циклическая зависимость
                // var user = mapper.Map<User>(remoteUser);
                localUser = new User()
                {
                    ID = (int)remoteUser.ID,
                    Login = remoteUser.Login,
                    Name = remoteUser.Name,
                    // TODO: С ролями не понятка
                    // ,Role = remoteUser.Role
                };
                context.Users.Add(localUser);
            }
        }

        private async Task Find(int id)
        {
            if (localUser?.ID != id)
                localUser = await context.Users.FindAsync(id);
        }

        public async Task DeleteLocalAsync(int id)
        {
            if (await LocalExist(id))
                context.Users.Remove(localUser);
        }

        public async Task<bool> LocalExist(int id)
        {
            await Find(id);
            return localUser != null;
        }

        public async Task DeleteRemoteAsync(int id) => await disk.DeleteUser((ID<UserDto>)id);

        public async Task UpdateRemoteAsync(int id)
        {
            await Find(id);
            UserDto user = new UserDto(
                id: new ID<UserDto>(localUser.ID),
                login: localUser.Login,
                name: localUser.Name);
            await disk.UnloadUser(user);
        }
    }
}