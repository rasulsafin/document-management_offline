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
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Utility.Factories;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly Synchronizer synchronizer;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<ConnectionService> logger;
        private readonly IFactory<Type, IConnection> connectionFactory;
        private readonly IFactory<IServiceScope, Type, IConnection> connectionScopedFactory;
        private readonly IFactory<IServiceScope, SynchronizingData> synchronizationDataFactory;
        private readonly IFactory<IServiceScope, ConnectionHelper> connectionHelperFactory;
        private readonly IRequestService requestQueue;
        private readonly ConnectionHelper helper;

        public ConnectionService(
            DMContext context,
            IMapper mapper,
            IRequestService requestQueue,
            ConnectionHelper helper,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ConnectionService> logger,
            IFactory<Type, IConnection> connectionFactory,
            IFactory<IServiceScope, Type, IConnection> connectionScopedFactory,
            IFactory<IServiceScope, SynchronizingData> synchronizationDataFactory,
            IFactory<IServiceScope, ConnectionHelper> connectionHelperFactory)
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
            this.connectionHelperFactory = connectionHelperFactory;
            logger.LogTrace("ConnectionService created");
        }

        public async Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto data)
        {
            logger.LogTrace("Add started with data = {@Data}", data);
            var connectionInfo = mapper.Map<ConnectionInfo>(data);
            logger.LogTrace("Mapped connection info = {@ConnectionInfo}", connectionInfo);
            await context.ConnectionInfos.AddAsync(connectionInfo);
            var user = await context.Users.FindAsync((int)data.UserID);
            logger.LogDebug("User found: {@User}", user);
            if (user == null)
                return ID<ConnectionInfoDto>.InvalidID;

            user.ConnectionInfo = connectionInfo;
            await context.SaveChangesAsync();

            return mapper.Map<ID<ConnectionInfoDto>>(connectionInfo.ID);
        }

        public async Task<RequestID> Connect(ID<UserDto> userID)
        {
            logger.LogInformation("Connect started with userID = {UserID}", userID);
            var id = Guid.NewGuid().ToString();
            var scope = serviceScopeFactory.CreateScope();
            var scopedHelper = connectionHelperFactory.Create(scope);

            Progress<double> progress = new Progress<double>(v => { requestQueue.SetProgress(v, id); });
            var src = new CancellationTokenSource();
            var task = Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        logger.LogTrace("Connection task started ({ID})", id);
                        var res = await scopedHelper.ConnectToRemote((int)userID, progress, src.Token);
                        logger.LogInformation("Connection end with {@Res}", res);
                        return res;
                    }
                    catch (OperationCanceledException ex)
                    {
                        logger.LogInformation("Connection canceled");
                        return new RequestResult(ex);
                    }
                    finally
                    {
                        scope.Dispose();
                        logger.LogTrace("Scope for Connect disposed");
                    }
                },
                TaskCreationOptions.LongRunning);
            requestQueue.AddRequest(id, task.Unwrap(), src);

            return await Task.FromResult(new RequestID(id));
        }

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            logger.LogTrace("Get started with userID = {UserID}", userID);
            var connectionInfoFromDb = await helper.GetConnectionInfoFromDb((int)userID);
            logger.LogTrace("Connection Info from DB: {@ConnectionInfoFromDb}", connectionInfoFromDb);
            return mapper.Map<ConnectionInfoDto>(connectionInfoFromDb);
        }

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID)
        {
            logger.LogTrace("GetRemoteConnectionStatus started with userID = {UserID}", userID);
            var connectionInfo = await helper.GetConnectionInfoFromDb((int)userID);
            logger.LogTrace("Connection Info from DB: {@ConnectionInfo}", connectionInfo);
            if (connectionInfo == null)
                return null;

            var connection = connectionFactory.Create(ConnectionCreator.GetConnection(connectionInfo.ConnectionType));
            return await connection.GetStatus(mapper.Map<ConnectionInfoExternalDto>(connectionInfo));
        }

        public async Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
        {
            logger.LogTrace(
                "GetEnumerationVariants started with userID = {UserID}, enumerationTypeID = {EnumerationTypeID}",
                userID,
                enumerationTypeID);
            var connectionInfo = await helper.GetConnectionInfoFromDb((int)userID);
            logger.LogTrace("Connection Info from DB: {@ConnectionInfo}", connectionInfo);
            if (connectionInfo == null)
                return null;
            var list = connectionInfo.EnumerationValues
                .Where(x => x.EnumerationValue.EnumerationTypeID == (int)enumerationTypeID)?
                .Select(x => mapper.Map<EnumerationValueDto>(x.EnumerationValue));
            logger.LogDebug("Enumeration values (id = {EnumerationTypeID}): {@List}", enumerationTypeID, list);
            return list;
        }

        public async Task<RequestID> Synchronize(ID<UserDto> userID)
        {
            logger.LogInformation("Synchronize started for user: {UserID}", userID);
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
            logger.LogTrace("Mapped info {@Info}", info);

            var id = Guid.NewGuid().ToString();
            Progress<double> progress = new Progress<double>(v => { requestQueue.SetProgress(v, id); });
            var src = new CancellationTokenSource();
            var task = Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        logger.LogTrace("Synchronization task started ({ID})", id);
                        var synchronizationResult = await synchronizer.Synchronize(data, connection, info, progress, src.Token);
                        logger.LogDebug(
                            "Synchronization ends with result: {@SynchronizationResult}",
                            synchronizationResult);
                        return new RequestResult(synchronizationResult.Count == 0);
                    }
                    finally
                    {
                        scope.Dispose();
                        logger.LogTrace("Scope for Synchronize disposed");
                    }
                },
                TaskCreationOptions.LongRunning);

            requestQueue.AddRequest(id, task.Unwrap(), src);

            return new RequestID(id);
        }
    }
}
