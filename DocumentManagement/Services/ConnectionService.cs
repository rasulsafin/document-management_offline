using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly DMContext context;

        public ConnectionService(DMContext context)
        {
            this.context = context;
        }

        private static RemoteConnectionInfoDto MapConnectionFromDb(Database.Models.ConnectionInfo info)
        {
            return new RemoteConnectionInfoDto()
            {
                ID = (ID<RemoteConnectionInfoDto>)info.ID,
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

        public async Task<IEnumerable<RemoteConnectionInfoDto>> GetAvailableConnections()
        {
            var connDb = await context.ConnectionInfos.ToListAsync();
            return connDb.Select(x => MapConnectionFromDb(x)).ToList();
        }

        public async Task<RemoteConnectionInfoDto> GetCurrentConnection(ID<UserDto> userId)
        {
            var connection = await context.Users.Include(x => x.ConnectionInfo)
                .Where(x => x.ID == (int)userId)
                .Select(x => x.ConnectionInfo)
                .FirstOrDefaultAsync();
            if (connection == null)
                return null;
            return MapConnectionFromDb(connection);
        }

        public Task<IEnumerable<EnumVariantDto>> GetEnumVariants(string dynamicFieldKey)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionStatusDto> GetRemoteConnectionStatus()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> LinkRemoteConnection(RemoteConnectionToCreateDto connectionInfo)
        {
            throw new NotImplementedException();
            
            //var currentConnection = await GetCurrentConnection();
            //if (currentConnection != null)
            //{
            //    //TODO: what to do?
            //}

            //var remote = await context.ConnectionInfos.FindAsync((int)connectionInfo.ID);
            //if (remote == null)
            //    throw new ArgumentException($"Remote connection with key {connectionInfo.ID} not found");
            //var authNames = DecodeAuthFieldNames(remote.AuthFieldNames);
            
            
            //// assign connection ID to user
            //var currentUserID = (int)userContext.CurrentUser.ID;
            //var user = await context.Users.FindAsync(currentUserID);
            //user.ConnectionInfoID = remote.ID;
            //context.Users.Update(user);
            //await context.SaveChangesAsync();
        }

        public Task<bool> Reconnect(RemoteConnectionToCreateDto connectionInfo)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ItemDto>> GetItems(IEnumerable<ID<ItemDto>> itemIds)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds)
        {
            throw new NotImplementedException();
        }
    }
}
