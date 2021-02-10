using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
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

        public async Task<bool> Add(ConnectionInfoToCreateDto data)
        {
            // TODO: Add connection info to db and link it to user and connection type?
            var connectionInfo = mapper.Map<ConnectionInfo>(data);
            context.ConnectionInfos.Add(connectionInfo);

            var user = await context.Users.FindAsync((int)data.UserID);
            user.ConnectionInfo = connectionInfo;

            await context.SaveChangesAsync();

            return true;
        }

        // TODO: Get Enums and ObjectiveTypes after connection?
        public async Task<ConnectionStatusDto> Connect(ID<UserDto> userID)
        {
            var connectionInfo = await GetConnectionInfo((int)userID);
            var connection = GetConnection(connectionInfo);
            var connectionInfoDto = mapper.Map<ConnectionInfoDto>(connectionInfo);

            return await connection.Connect(connectionInfoDto);
        }

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            var connectionInfoFromDb = await GetConnectionInfo((int)userID);
            return mapper.Map<ConnectionInfoDto>(connectionInfoFromDb);
        }

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus()
        {
            throw new System.NotImplementedException();
        }

        //public async Task<bool> StartSyncronization(ID<UserDto> userID)
        //{
        //    var connection = GetConnection(await GetConnectionInfo((int)userID));
        //    return await connection.StartSyncronization();
        //}

        //public async Task<bool> StopSyncronization(ID<UserDto> userID)
        //{
        //    var connection = GetConnection(await GetConnectionInfo((int)userID));
        //    return await connection.StopSyncronization();
        //}

        private async Task<ConnectionInfo> GetConnectionInfo(int userID)
        {
            var user = await context.Users
                .Include(x => x.ConnectionInfo)
                .ThenInclude(x => x.ConnectionType)
                .Where(x => x.ID == userID)
                .FirstOrDefaultAsync();
            return await context.ConnectionInfos.FindAsync(user.ConnectionInfoID);
        }

        private IConnection GetConnection(ConnectionInfo connectionInfo)
        {
            var type = mapper.Map<ConnectionTypeDto>(connectionInfo.ConnectionType);
            return connectionFactory.GetConnection(type);
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
        //    //   
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
