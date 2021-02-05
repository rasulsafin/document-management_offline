using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace DocumentManagement.Connection
{
    public class ConnectionManager
    {
        public async Task<(bool, string)> AuthorizeAtRemote(RemoteConnectionInfoDto info, dynamic param)
        {
            var connection = GetConnection(info.ConnectionType);
            return await connection.Connect(param);
        }

        public async Task GetProgressSyncronization(RemoteConnectionInfoDto info)
        {
            var connection = GetConnection(info.ConnectionType);
            await connection.GetProgressSyncronization();
        }

        public async Task<bool> StartSyncronization(RemoteConnectionInfoDto info)
        {
            var connection = GetConnection(info.ConnectionType);
            return await connection.StartSyncronization();
        }

        public async Task StopSyncronization(RemoteConnectionInfoDto info)
        {
            var connection = GetConnection(info.ConnectionType);
            await connection.StopSyncronization();
        }

        private IConnection GetConnection(ConnectionTypeDto type)
        {
            throw new NotImplementedException();
        }
    }
}
