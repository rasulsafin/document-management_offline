using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizator
{
    internal class UserSynchronizer : ISynchronizer
    {
        private DiskManager disk;
        private DMContext context;
        private DbSet<User> users;
        private UserDto remoteUser;

        public UserSynchronizer(DiskManager yandex, DMContext context)
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

        public List<ISynchronizer> GetSubSynchronizes(int id) => null;


        public void LoadLocalCollect()
        {
            users = context.Users;
        }
        public void SaveLocalCollect()
        {
            context.SaveChanges();
        }


        public async Task<bool> RemoteExistAsync(int id)
        {
            remoteUser = await disk.GetUserAsync((ID<UserDto>)id);
            return remoteUser != null;
        }
        public async Task DownloadAndUpdateAsync(int id)
        {
            if ((int)remoteUser.ID != id)
                remoteUser = await disk.GetUserAsync((ID<UserDto>)id);



            var user = await users.FirstAsync();
            //user.
            throw new System.NotImplementedException();
        }
        public void DeleteLocal(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteRemoteAsync(int id)
        {
            throw new System.NotImplementedException();
        }


        

        



        public bool LocalExist(int id)
        {
            throw new System.NotImplementedException();
        }



        

        public Task UpdateRemoteAsync(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}