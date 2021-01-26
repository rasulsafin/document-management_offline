using System.Threading.Tasks;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;

namespace DocumentManagement.Connection.Tests
{
    public class DiskTest : IDiskManager
    {
        public DiskTest()
        {
        }

        public bool RunDelete { get; private set; }

        public bool RunPull { get; private set; }

        public bool RunPush { get; private set; }

        public int LastId { get; private set; }

        public string NameType { get; private set; }

        public ProjectDto Project { get; set; }

        public UserSychro.UserSyncModel User { get; set; }

        public Task Delete<T>(string id)
        {
            RunDelete = true;
            NameType = typeof(T).Name;
            if (int.TryParse(id, out int num))
            {
                LastId = num;
            }

            return Task.CompletedTask;
        }

        public Task<T> Pull<T>(string id)
        {
            RunPull = true;
            NameType = typeof(T).Name;
            if (int.TryParse(id, out int num))
            {
                LastId = num;
                if (Project is T prj)
                    return Task.FromResult(prj);
                if (User is T usr)
                    return Task.FromResult(usr);
            }

            return Task.FromResult<T>(default);
        }

        public Task<bool> Push<T>(T @object, string id)
        {
            RunPush = true;
            NameType = typeof(T).Name;
            if (int.TryParse(id, out int num))
            {
                LastId = num;
                if (@object is ProjectDto prj)
                {
                    Project = prj;
                    return Task.FromResult(true);
                }

                if (@object is UserSychro.UserSyncModel usr)
                {
                    User = usr;
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }

}
