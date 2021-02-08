using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Interface.SyncData;
using MRS.DocumentManagement.Connection;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private static SyncManager syncManager;
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IServiceScopeFactory factoryScope;
        private readonly ConnectionManager connectionManager;

        public ConnectionService(DMContext context, IMapper mapper, ConnectionManager connectionManager, IServiceScopeFactory factory)
        {
            this.context = context;
            this.mapper = mapper;
            factoryScope = factory;
            syncManager ??= SyncManager.Instance;
            this.connectionManager = connectionManager;
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

        public Task<bool> LinkRemoteConnection(RemoteConnectionToCreateDto connectionInfo)
        {
            throw new NotImplementedException();
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

        public Task<bool> StartSync()
        {
            if (!syncManager.Initilize)
            {
                // TODO: В будушем это надо вытаскивать из базы опираясь на пользователя
                var connection = context.ConnectionInfos.Find(1);
                if (connection == null)
                {
                    connection = new ConnectionInfo();
                    connection.Name = SyncManager.YANDEX;
                    context.ConnectionInfos.Add(connection);
                    context.SaveChanges();
                }

                syncManager.Initialization(mapper.Map<RemoteConnectionInfoDto>(connection));
                return Task.FromResult(false);
            }
            else
            {
                var cont = factoryScope.CreateScope().ServiceProvider.GetService<DMContext>();
                if (!syncManager.NowSync)
                    syncManager.StartSync(cont, mapper);
                return Task.FromResult(true);
            }
        }

        public void StopSync()
        {
            syncManager.StopSync();
        }

        public Task<ProgressSync> GetProgressSync()
        {
            return Task.FromResult(syncManager.GetProgressSync());
        }

        public Task TokenYandexDisk(string access_token)
        {
            var connection = context.ConnectionInfos.First(x => x.Name == SyncManager.YANDEX);
            var connectionDto = mapper.Map<RemoteConnectionInfoDto>(connection);
            connectionDto.AuthFieldNames = new List<string>() { access_token };
            connection = mapper.Map<ConnectionInfo>(connectionDto);
            context.SaveChanges();
            syncManager.Initialization(mapper.Map<RemoteConnectionInfoDto>(connection));
            return Task.CompletedTask;
        }

        private RemoteConnectionInfoDto MapConnectionFromDb(Database.Models.ConnectionInfo info)
            => mapper.Map<RemoteConnectionInfoDto>(info);
    }
}
