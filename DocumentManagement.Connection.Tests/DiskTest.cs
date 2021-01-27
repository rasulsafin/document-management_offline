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

        public UserSynchro.UserSync User { get; set; }

        public ItemDto Item { get; set; }

        public ObjectiveDto Objective { get; internal set; }

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
                if (Item is T itm)
                    return Task.FromResult(itm);
                if (Objective is T obj)
                    return Task.FromResult(obj);
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
            }

            return Task.FromResult(false);
        }
    }

}
