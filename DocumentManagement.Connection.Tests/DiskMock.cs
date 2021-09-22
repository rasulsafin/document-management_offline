using System.Threading.Tasks;
using Brio.Docs.Connection.Utils;
using Brio.Docs.Interface.Dtos;
using Brio.Docs.Synchronizer;

namespace DocumentManagement.Connection.Tests
{
    public class DiskMock : ICloudManager
    {
        public DiskMock()
        {
        }

        public bool RunDelete { get; private set; }

        public bool RunPull { get; private set; }

        public bool RunPush { get; private set; }

        public bool RunPullFile { get; private set; }

        public bool RunPushFile { get; private set; }

        public bool RunDeleteFile { get; internal set; }

        public int LastId { get; private set; }

        public string NameType { get; private set; }

        public ProjectDto Project { get; set; }

        public UserSynchro.UserSync User { get; set; }

        public ItemDto Item { get; set; }

        public ObjectiveDto Objective { get; internal set; }

        public ObjectiveTypeDto ObjectiveType { get; set; }

        public Task<bool> Delete<T>(string id)
        {
            RunDelete = true;
            NameType = typeof(T).Name;
            if (int.TryParse(id, out int num))
            {
                LastId = num;
            }

            return Task.FromResult(true);
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
                if (Item is T itm)
                    return Task.FromResult(itm);
                if (Objective is T obj)
                    return Task.FromResult(obj);
                if (ObjectiveType is T obt)
                    return Task.FromResult(obt);
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

                if (@object is UserSynchro.UserSync usr)
                {
                    User = usr;
                    return Task.FromResult(true);
                }

                if (@object is ItemDto itm)
                {
                    Item = itm;
                    return Task.FromResult(true);
                }

                if (@object is ObjectiveDto obj)
                {
                    Objective = obj;
                    return Task.FromResult(true);
                }

                if (@object is ObjectiveTypeDto qbt)
                {
                    ObjectiveType = qbt;
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public Task<bool> DeleteFile(string path)
        {
            RunDeleteFile = true;
            if (Item.ExternalItemId == path)
                return Task.FromResult(true);
            return Task.FromResult(false);
        }

        public Task<string> PushFile(string remoteDirName, string localDirName, string fileName)
        {
            RunPushFile = true;
            return Task.FromResult(string.Empty);
        }

        public Task<bool> PullFile(string href, string fileName)
        {
            RunPullFile = true;
            return Task.FromResult(true);
        }

        public Task<ConnectionStatusDto> GetStatusAsync()
        {
             throw new System.NotImplementedException();
        }
    }
}
