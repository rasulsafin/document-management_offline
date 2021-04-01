using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Synchronization;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Utility.Factories;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly Synchronizer synchronizer;
        private readonly IServiceScopeFactory  serviceScopeFactory;
        private readonly ILogger<ConnectionService> logger;
        private readonly IFactory<Type, IConnection> connectionFactory;
        private readonly IFactory<IServiceScope, Type, IConnection> connectionScopedFactory;
        private readonly IFactory<IServiceScope, SynchronizingData> synchronizationDataFactory;
        private readonly IRequestService requestQueue;
        private readonly ConnectionHelper helper;

        public ConnectionService(
            DMContext context,
            IMapper mapper,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ConnectionService> logger,
            IFactory<Type, IConnection> connectionFactory,
            IFactory<IServiceScope, Type, IConnection> connectionScopedFactory,
            IFactory<IServiceScope, SynchronizingData> synchronizationDataFactory)
        {
            this.context = context;
            this.mapper = mapper;
            this.serviceScopeFactory = serviceScopeFactory;
            this.requestQueue = requestQueue;
            this.helper = helper;
            synchronizer = new Synchronizer();
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.connectionScopedFactory = connectionScopedFactory;
            this.synchronizationDataFactory = synchronizationDataFactory;
        }

        public async Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto data)
        {
            var connectionInfo = mapper.Map<ConnectionInfo>(data);
            context.ConnectionInfos.Add(connectionInfo);
            var user = await context.Users.FindAsync((int)data.UserID);
            if (user == null)
                return ID<ConnectionInfoDto>.InvalidID;
            user.ConnectionInfo = connectionInfo;

            await context.SaveChangesAsync();

            return mapper.Map<ID<ConnectionInfoDto>>(connectionInfo.ID);
        }

        public async Task<RequestID> Connect(ID<UserDto> userID)
        {
            var id = Guid.NewGuid().ToString();
            var scope = serviceScopeFactory.CreateScope();
            var scopedContext = scope.ServiceProvider.GetRequiredService<DMContext>();
            var scopedMapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var scopedhelper = new ConnectionHelper(scopedContext, scopedMapper);

            Progress<double> progress = new Progress<double>(v => { requestQueue.SetProgress(v, id); });
            var src = new CancellationTokenSource();
            var task = Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        var res = await scopedhelper.ConnectToRemote((int)userID, progress, src.Token);
                        return res;
                    }
                    catch (OperationCanceledException ex)
                    {
                        return new RequestResult(ex);
                    }
                    finally
                    {
                        scope.Dispose();
                    }
                },
                TaskCreationOptions.LongRunning);
            requestQueue.AddRequest(id, task.Unwrap(), src);

            return await Task.FromResult(new RequestID(id));
        }

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            var connectionInfoFromDb = await helper.GetConnectionInfoFromDb((int)userID);
            return mapper.Map<ConnectionInfoDto>(connectionInfoFromDb);
        }

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID)
        {
            var connectionInfo = await helper.GetConnectionInfoFromDb((int)userID);
            if (connectionInfo == null)
                return null;

            var connection = connectionFactory.Create(ConnectionCreator.GetConnection(connectionInfo.ConnectionType));

            return await connection.GetStatus(mapper.Map<ConnectionInfoExternalDto>(connectionInfo));
        }

        public async Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
        {
            var connectionInfo = await helper.GetConnectionInfoFromDb((int)userID);
            if (connectionInfo == null)
                return null;
            var list = connectionInfo.EnumerationValues
                .Where(x => x.EnumerationValue.EnumerationTypeID == (int)enumerationTypeID)?
                .Select(x => mapper.Map<EnumerationValueDto>(x.EnumerationValue));

            return list;
        }

        public async Task<RequestID> Synchronize(ID<UserDto> userID)
        {
            var iUserID = (int)userID;
            var user = await context.Users
                .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.ConnectionType)
                        .ThenInclude(x => x.AppProperties)
                .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.ConnectionType)
                        .ThenInclude(x => x.AuthFieldNames)
                .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.AuthFieldValues)
                .FirstOrDefaultAsync(x => x.ID == iUserID);
            if (user == null)
                throw new ArgumentNullException();

            var scope = serviceScopeFactory.CreateScope();
            var data = synchronizationDataFactory.Create(scope);

            data.User = user;
            data.ProjectsFilter = x => x.Users.Any(u => u.UserID == iUserID);
            data.ObjectivesFilter = x => x.Project.Users.Any(u => u.UserID == iUserID);

            var connection = connectionScopedFactory.Create(
                scope,
                ConnectionCreator.GetConnection(user.ConnectionInfo.ConnectionType));
            var info = mapper.Map<ConnectionInfoExternalDto>(user.ConnectionInfo);

            var id = Guid.NewGuid().ToString();
            Progress<double> progress = new Progress<double>(v => { requestQueue.SetProgress(v, id); });
            var src = new CancellationTokenSource();
            var task = Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        var synchronizationResult = await synchronizer.Synchronize(data, connection, info, progress, src.Token);
                        return new RequestResult(synchronizationResult.Count == 0);
                    }
                    finally
                    {
                        scope.Dispose();
                    }
                },
                TaskCreationOptions.LongRunning);

            requestQueue.AddRequest(id, task.Unwrap(), src);

            return new RequestID(id);
        }
    }
}
