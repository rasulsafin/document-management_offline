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

        public async Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto data)
        {
            var connectionInfo = mapper.Map<ConnectionInfo>(data);
            context.ConnectionInfos.Add(connectionInfo);
            var user = await context.Users.FindAsync((int)data.UserID);
            user.ConnectionInfo = connectionInfo;

            await context.SaveChangesAsync();

            return mapper.Map<ID<ConnectionInfoDto>>(connectionInfo.ID);
        }

        // TODO: Get Enums and ObjectiveTypes after connection?
        public async Task<ConnectionStatusDto> Connect(ID<UserDto> userID)
        {
            var connectionInfo = await GetConnectionInfoFromDb((int)userID);
            var connection = GetConnection(connectionInfo);
            var connectionInfoDto = mapper.Map<ConnectionInfoDto>(connectionInfo);

            return await connection.Connect(connectionInfoDto);
        }

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            var connectionInfoFromDb = await GetConnectionInfoFromDb((int)userID);
            return mapper.Map<ConnectionInfoDto>(connectionInfoFromDb);
        }

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID)
        {
            var connectionInfo = await GetConnectionInfoFromDb((int)userID);
            var connection = GetConnection(connectionInfo);

            return await connection.GetStatus();
        }

        private async Task<ConnectionInfo> GetConnectionInfoFromDb(int userID)
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
    }
}
