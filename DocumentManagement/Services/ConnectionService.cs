using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DocumentManagement.Database;
using DocumentManagement.Interface.Models;
using DocumentManagement.Interface.Services;
using System.Linq;

namespace DocumentManagement.Services
{
    internal class ConnectionService : IConnectionService
    {
        private readonly DMContext context;
        private readonly IUserContext userContext;

        public ConnectionService(DMContext context, IUserContext userContext)
        {
            this.context = context;
            this.userContext = userContext;
        }

        private static RemoteConnectionInfo MapConnectionFromDb(Database.Models.ConnectionInfo info)
        {
            return new RemoteConnectionInfo()
            {
                ID = (ID<RemoteConnectionInfo>)info.ID,
                ServiceName = info.Name,
                AuthFieldNames = DecodeAuthFieldNames(info.AuthFieldNames)
            };
        }

        private static string EncodeAuthFieldNames(IEnumerable<string> names)
        {
            var lnames = names.ToList();
            return System.Text.Json.JsonSerializer.Serialize(lnames);
        }

        private static List<string> DecodeAuthFieldNames(string encoded)
        {
            var names = new List<string>();
            if (!string.IsNullOrEmpty(encoded))
                names = System.Text.Json.JsonSerializer.Deserialize<List<string>>(encoded);
            return names;
        }

        public async Task<IEnumerable<RemoteConnectionInfo>> GetAvailableConnections()
        {
            var connDb = await context.ConnectionInfos.ToListAsync();
            return connDb.Select(x => MapConnectionFromDb(x)).ToList();
        }

        public async Task<RemoteConnectionInfo> GetCurrentConnection()
        {
            var currentUserID = (int)userContext.CurrentUser.ID;
            var connection = await context.Users.Include(x => x.ConnectionInfo)
                .Where(x => x.ID == currentUserID)
                .Select(x => x.ConnectionInfo)
                .FirstOrDefaultAsync();
            if (connection == null)
                return null;
            return MapConnectionFromDb(connection);
        }

        public Task<IEnumerable<EnumVariant>> GetEnumVariants(string dynamicFieldKey)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionStatus> GetRemoteConnectionStatus()
        {
            throw new NotImplementedException();
        }

        public async Task LinkRemoteConnection(RemoteConnectionToCreate connectionInfo)
        {
            throw new NotImplementedException();
            
            var currentConnection = await GetCurrentConnection();
            if (currentConnection != null)
            {
                //TODO: what to do?
            }

            var remote = await context.ConnectionInfos.FindAsync((int)connectionInfo.ID);
            if (remote == null)
                throw new ArgumentException($"Remote connection with key {connectionInfo.ID} not found");
            var authNames = DecodeAuthFieldNames(remote.AuthFieldNames);
            
            
            // assign connection ID to user
            var currentUserID = (int)userContext.CurrentUser.ID;
            var user = await context.Users.FindAsync(currentUserID);
            user.ConnectionInfoID = remote.ID;
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }

        public Task Reconnect(RemoteConnectionToCreate connectionInfo)
        {
            throw new NotImplementedException();
        }
    }
}
