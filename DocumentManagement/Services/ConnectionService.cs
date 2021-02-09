using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ConnectionCreator connectionFactory;

        public ConnectionService(DMContext context, IMapper mapper, ConnectionCreator connectionFactory)
        {
            this.context = context;
            this.mapper = mapper;
            this.connectionFactory = connectionFactory;
        }

        public async Task<bool> Add(ConnectionInfoToCreateDto connectionInfo)
        {
            throw new System.NotImplementedException();
        }

        // TODO: Get Enums and ObjectiveTypes after connection?
        public async Task<ConnectionStatusDto> Connect(ID<UserDto> userID)
        {
            var user = await context.Users.FindAsync((int)userID);
            var connectionInfo = await context.ConnectionInfos.FindAsync(user.ConnectionInfoID);
            var type = mapper.Map<ConnectionTypeDto>(connectionInfo.ConnectionType);

            var connection = connectionFactory.GetConnection(type);
            return await connection.Connect(connectionInfo.AuthFieldValues);
        }

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus()
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> StartSyncronization(ID<UserDto> userID)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> StopSyncronization(ID<UserDto> userID)
        {
            throw new System.NotImplementedException();
        }

        //private ConnectionInfoDto MapConnectionFromDb(Database.Models.ConnectionInfo info) 
        //    => mapper.Map<ConnectionInfoDto>(info);

        //private static string EncodeAuthFieldNames(IEnumerable<string> names)
        //{
        //    var lnames = names.ToList();
        //    return System.Text.Json.JsonSerializer.Serialize(lnames);
        //}

        //public async Task<IEnumerable<ConnectionInfoDto>> GetAvailableConnections()
        //{
        //    var connDb = await context.ConnectionInfos.ToListAsync();
        //    return connDb.Select(MapConnectionFromDb).ToList();
        //}

        //public async Task<ConnectionInfoDto> GetCurrentConnection(ID<UserDto> userId)
        //{
        //    var connection = await context.Users.Include(x => x.ConnectionInfo)
        //        .Where(x => x.ID == (int)userId)
        //        .Select(x => x.ConnectionInfo)
        //        .FirstOrDefaultAsync();
        //    return connection != null ? MapConnectionFromDb(connection) : null;
        //}

        //public Task<IEnumerable<EnumVariantDto>> GetEnumVariants(string dynamicFieldKey)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<ConnectionStatusDto> GetRemoteConnectionStatus()
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<bool> LinkRemoteConnection(ConnectionInfoToCreateDto connectionInfo)
        //{
        //    throw new NotImplementedException();

        //    //var currentConnection = await GetCurrentConnection();
        //    //if (currentConnection != null)
        //    //{
        //    //    //TODO: what to do?
        //    //}

        //    //var remote = await context.ConnectionInfos.FindAsync((int)connectionInfo.ID);
        //    //if (remote == null)
        //    //    throw new ArgumentException($"Remote connection with key {connectionInfo.ID} not found");
        //    //var authNames = DecodeAuthFieldNames(remote.AuthFieldNames);


        //    //// assign connection ID to user
        //    //var currentUserID = (int)userContext.CurrentUser.ID;
        //    //var user = await context.Users.FindAsync(currentUserID);
        //    //user.ConnectionInfoID = remote.ID;
        //    //context.Users.Update(user);
        //    //await context.SaveChangesAsync();
        //}

        //public Task<bool> Reconnect(ConnectionInfoToCreateDto connectionInfo)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<IEnumerable<ItemDto>> GetItems(IEnumerable<ID<ItemDto>> itemIds)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<(bool, string)> AuthorizeAtRemote(ConnectionInfoDto info, dynamic param)
        //{
        //    var connection = GetConnection(info.ConnectionType);
        //    return await connection.Connect(param);
        //}

        //public async Task GetProgressSyncronization(ConnectionInfoDto info)
        //{
        //    var connection = GetConnection(info.ConnectionType);
        //    await connection.GetProgressSyncronization();
        //}

        //public async Task<bool> StartSyncronization(ConnectionInfoDto info)
        //{
        //    var connection = GetConnection(info.ConnectionType);
        //    return await connection.StartSyncronization();
        //}

        //public async Task StopSyncronization(ConnectionInfoDto info)
        //{
        //    var connection = GetConnection(info.ConnectionType);
        //    await connection.StopSyncronization();
        //}
    }
}
