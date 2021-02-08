using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MRS.DocumentManagement.Connection;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ConnectionManager connectionManager;

        public ConnectionService(DMContext context, IMapper mapper, ConnectionManager connectionManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.connectionManager = connectionManager;
        }

        private RemoteConnectionInfoDto MapConnectionFromDb(Database.Models.ConnectionInfo info) 
            => mapper.Map<RemoteConnectionInfoDto>(info);

        private static string EncodeAuthFieldNames(IEnumerable<string> names)
        {
            var lnames = names.ToList();
            return System.Text.Json.JsonSerializer.Serialize(lnames);
        }

        public async Task<IEnumerable<RemoteConnectionInfoDto>> GetAvailableConnections()
        {
            var connDb = await context.ConnectionInfos.ToListAsync();
            return connDb.Select(MapConnectionFromDb).ToList();
        }

        public async Task<RemoteConnectionInfoDto> GetCurrentConnection(ID<UserDto> userId)
        {
            var connection = await context.Users.Include(x => x.ConnectionInfo)
                .Where(x => x.ID == (int)userId)
                .Select(x => x.ConnectionInfo)
                .FirstOrDefaultAsync();
            return connection != null ? MapConnectionFromDb(connection) : null;
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
