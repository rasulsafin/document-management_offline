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
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<ConnectionService> logger;
        private readonly IFactory<Type, IConnection> connectionFactory;
        private readonly IFactory<IServiceScope, Type, IConnection> connectionScopedFactory;
        private readonly IFactory<IServiceScope, ConnectionHelper> connectionHelperFactory;
        private readonly IFactory<IServiceScope, Synchronizer> synchronizerFactory;
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
            IFactory<IServiceScope, ConnectionHelper> connectionHelperFactory,
            IFactory<IServiceScope, Synchronizer> synchronizerFactory)
        {
            this.context = context;
            this.mapper = mapper;
            this.serviceScopeFactory = serviceScopeFactory;
            this.requestQueue = requestQueue;
            this.helper = helper;
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.connectionScopedFactory = connectionScopedFactory;
            this.connectionHelperFactory = connectionHelperFactory;
            this.synchronizerFactory = synchronizerFactory;
        }

        public async Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto data)
        {
            try
            {
                var connectionInfo = mapper.Map<ConnectionInfo>(data);
                context.ConnectionInfos.Add(connectionInfo);
                var user = await context.Users.FindAsync((int)data.UserID);
                if (user == null)
                    throw new ArgumentNullException($"User with key {data.UserID} was not found");

                user.ConnectionInfo = connectionInfo;
                await context.SaveChangesAsync();

                return mapper.Map<ID<ConnectionInfoDto>>(connectionInfo.ID);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add new ConnectionInfo {@Data}", data);
                throw;
            }
        }

        public async Task<RequestID> Connect(ID<UserDto> userID)
        {
            try
            {
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
                            var res = await scopedHelper.ConnectToRemote((int)userID, progress, src.Token);
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't start the connection process to remote with user id {UserID}", userID);
                throw;
            }
        }

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            try
            {
                var connectionInfoFromDb = await helper.GetConnectionInfoFromDb((int)userID);
                return mapper.Map<ConnectionInfoDto>(connectionInfoFromDb);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get connection info with user id {UserID}", userID);
                throw;
            }
        }

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID)
        {
            try
            {
                var connectionInfo = await helper.GetConnectionInfoFromDb((int)userID);
                if (connectionInfo == null)
                    throw new ArgumentNullException($"ConnectionInfo with user's id {userID} was not found");

                var connection = connectionFactory.Create(ConnectionCreator.GetConnection(connectionInfo.ConnectionType));

                return await connection.GetStatus(mapper.Map<ConnectionInfoExternalDto>(connectionInfo));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get status info with user id {UserID}", userID);
                throw;
            }
        }

        public async Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
        {
            try
            {
                var connectionInfo = await helper.GetConnectionInfoFromDb((int)userID);
                if (connectionInfo == null)
                    throw new ArgumentNullException($"ConnectionInfo with user's id {userID} was not found");

                var list = connectionInfo.EnumerationValues
                    .Where(x => x.EnumerationValue.EnumerationTypeID == (int)enumerationTypeID)?
                    .Select(x => mapper.Map<EnumerationValueDto>(x.EnumerationValue));

                return list;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get Enumeration Variants with user id {UserID} and enumeration type id {EnumerationTypeID}", userID, enumerationTypeID);
                throw;
            }
        }

        public async Task<RequestID> Synchronize(ID<UserDto> userID)
        {
            try
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
                    throw new ArgumentNullException($"User with key {userID} was not found");

                var scope = serviceScopeFactory.CreateScope();
                var synchronizer = synchronizerFactory.Create(scope);

                var data = new SynchronizingData
                {
                    User = user,
                    ProjectsFilter = x => x.Users.Any(u => u.UserID == iUserID),
                    ObjectivesFilter = x => x.Project.Users.Any(u => u.UserID == iUserID),
                };

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
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't start the synchronizing process with user id {UserID}", userID);
                throw;
            }
        }
    }
}
