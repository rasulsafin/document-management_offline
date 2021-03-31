using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Dictionary<string, Task<ICollection<SynchronizingResult>>> SYNCHRONIZATIONS =
            new Dictionary<string, Task<ICollection<SynchronizingResult>>>();

        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly Synchronizer synchronizer;
        private readonly IServiceScopeFactory  serviceScopeFactory;
        private readonly ILogger<ConnectionService> logger;
        private readonly IFactory<Type, IConnection> connectionFactory;
        private readonly IFactory<IServiceScope, Type, IConnection> connectionScopedFactory;
        private readonly IFactory<IServiceScope, SynchronizingData> synchronizationDataFactory;

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

        public async Task<ConnectionStatusDto> Connect(ID<UserDto> userID)
        {
            User user = await FindUserFromDb((int)userID);
            if (user == null)
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = "Пользователь отсутвует в базе!", };

            // Get connection info from user
            var connectionInfo = await GetConnectionInfoFromDb(user);
            if (connectionInfo == null)
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = "Подключение не найдено! (connectionInfo == null)", };

            var connection = connectionFactory.Create(ConnectionCreator.GetConnection(connectionInfo.ConnectionType));
            var connectionInfoExternalDto = mapper.Map<ConnectionInfoExternalDto>(connectionInfo);

            ConnectionStatusDto status;

            // Connect to Remote
            try
            {
                status = await connection.Connect(connectionInfoExternalDto);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't connect with info {@ConnectionInfo}", connectionInfo);
                return null;
            }

            // Update connection info
            connectionInfoExternalDto = await connection.UpdateConnectionInfo(connectionInfoExternalDto);
            connectionInfo = mapper.Map(connectionInfoExternalDto, connectionInfo);

            user.ExternalID = connectionInfoExternalDto.UserExternalID;

            context.Update(connectionInfo);
            await context.SaveChangesAsync();

            // Update types stored in connection info
            await UpdateEnumerationObjects(connectionInfo, connectionInfoExternalDto);

            // Update objective types stored in connection type
            foreach (var externalType in connectionInfoExternalDto.ConnectionType.ObjectiveTypes)
            {
                var dbType = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault(x => x.ExternalId == externalType.ExternalId);
                if (dbType != null)
                {
                    dbType.DefaultDynamicFields = mapper.Map<ICollection<DynamicFieldInfo>>(externalType.DefaultDynamicFields);
                    dbType.Name = externalType.Name;
                } else
                {
                    var newType = mapper.Map<ObjectiveType>(externalType);
                    connectionInfo.ConnectionType.ObjectiveTypes.Add(newType);
                }
            }

            context.Update(connectionInfo);
            await context.SaveChangesAsync();

            return status;
        }

        public async Task<ConnectionInfoDto> Get(ID<UserDto> userID)
        {
            var connectionInfoFromDb = await GetConnectionInfoFromDb((int)userID);
            return mapper.Map<ConnectionInfoDto>(connectionInfoFromDb);
        }

        public async Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID)
        {
            var connectionInfo = await GetConnectionInfoFromDb((int)userID);
            if (connectionInfo == null)
                return null;

            var connection = connectionFactory.Create(ConnectionCreator.GetConnection(connectionInfo.ConnectionType));

            return await connection.GetStatus(mapper.Map<ConnectionInfoExternalDto>(connectionInfo));
        }

        public async Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
        {
            var connectionInfo = await GetConnectionInfoFromDb((int)userID);
            if (connectionInfo == null)
                return null;
            var list = connectionInfo.EnumerationValues
                .Where(x => x.EnumerationValue.EnumerationTypeID == (int)enumerationTypeID)?
                .Select(x => mapper.Map<EnumerationValueDto>(x.EnumerationValue));

            return list;
        }

        public async Task<string> Synchronize(ID<UserDto> userID)
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
                return null;
            var id = Guid.NewGuid().ToString();

            var scope = serviceScopeFactory.CreateScope();
            var data = synchronizationDataFactory.Create(scope);

            data.User = user;
            data.ProjectsFilter = x => x.Users.Any(u => u.UserID == iUserID);
            data.ObjectivesFilter = x => x.Project.Users.Any(u => u.UserID == iUserID);

            var connection = connectionScopedFactory.Create(
                scope,
                ConnectionCreator.GetConnection(user.ConnectionInfo.ConnectionType));
            var info = mapper.Map<ConnectionInfoExternalDto>(user.ConnectionInfo);

            var task = Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        var synchronizationResult = await synchronizer.Synchronize(data, connection, info);
                        //TODO: Delete or fix. This method is crushing everithing.
                        //await UpdateConnectionInfo(scopedContext, info, user.ConnectionInfo);
                        return synchronizationResult;
                    }
                    finally
                    {
                        scope.Dispose();
                    }
                },
                TaskCreationOptions.LongRunning);
            SYNCHRONIZATIONS.Add(id, task.Unwrap());

            return id;
        }

        public Task<bool> IsSynchronizationComplete(string synchronizationID)
        {
            if (SYNCHRONIZATIONS.TryGetValue(synchronizationID, out var task))
            {
                var result = task.IsCompleted;
                return Task.FromResult(result);
            }

            throw new ArgumentException($"The synchronization {synchronizationID} doesn't exist");
        }

        public Task<bool> GetSynchronizationResult(string synchronizationID)
        {
            if (SYNCHRONIZATIONS.TryGetValue(synchronizationID, out var task))
            {
                var result = task.Result.Count <= 0;
                SYNCHRONIZATIONS.Remove(synchronizationID);
                return Task.FromResult(result);
            }

            throw new ArgumentException($"The synchronization {synchronizationID} doesn't exist");
        }

        #region private methods
        private async Task<ConnectionInfo> GetConnectionInfoFromDb(int userID)
        {
            User user = await FindUserFromDb(userID);
            return await GetConnectionInfoFromDb(user);
        }

        private async Task<User> FindUserFromDb(int userID)
        {
            return await context.Users
                            .Include(x => x.ConnectionInfo)
                            .FirstOrDefaultAsync(x => x.ID == userID);
        }

        private async Task<ConnectionInfo> GetConnectionInfoFromDb(User user)
        {
            if (user == null)
                return null;

            var info = await context.ConnectionInfos
                .Include(x => x.ConnectionType)
                    .ThenInclude(x => x.AppProperties)
                .Include(x => x.ConnectionType)
                    .ThenInclude(x => x.ObjectiveTypes)
                        .ThenInclude(x => x.DefaultDynamicFields)
                .Include(x => x.ConnectionType)
                    .ThenInclude(x => x.AuthFieldNames)
                .Include(x => x.EnumerationTypes)
                    .ThenInclude(x => x.EnumerationType)
                .Include(x => x.EnumerationValues)
                    .ThenInclude(x => x.EnumerationValue)
                .Include(x => x.AuthFieldValues)
                .FirstOrDefaultAsync(x => x.ID == user.ConnectionInfoID);

            return info;
        }

        private async Task<EnumerationType> LinkEnumerationTypes(EnumerationTypeExternalDto enumType, ConnectionInfo connectionInfo)
        {
            var enumTypeDb = await CheckEnumerationTypeToLink(enumType, (int)connectionInfo.ID);
            if (enumTypeDb != null)
            {
                connectionInfo.EnumerationTypes.Add(new ConnectionInfoEnumerationType
                {
                    ConnectionInfoID = connectionInfo.ID,
                    EnumerationTypeID = enumTypeDb.ID,
                });

                await context.SaveChangesAsync();
            }

            return enumTypeDb;
        }

        private async Task LinkEnumerationValues(EnumerationValueExternalDto enumVal, EnumerationType type, ConnectionInfo connectionInfo)
        {
            var enumValueDb = await CheckEnumerationValueToLink(enumVal, type, (int)connectionInfo.ID);
            if (enumValueDb == null)
                return;

            connectionInfo.EnumerationValues.Add(new ConnectionInfoEnumerationValue
            {
                ConnectionInfoID = connectionInfo.ID,
                EnumerationValueID = enumValueDb.ID,
            });

            await context.SaveChangesAsync();
        }

        private async Task<EnumerationType> CheckEnumerationTypeToLink(EnumerationTypeExternalDto enumTypeDto, int connectionInfoID)
        {
            var enumTypeDb = await context.EnumerationTypes
                    .FirstOrDefaultAsync(i => i.ExternalId == enumTypeDto.ExternalID);

            if (enumTypeDb == null)
            {
                enumTypeDb = mapper.Map<EnumerationType>(enumTypeDto);
                var connectionType = context.ConnectionInfos.Include(x => x.ConnectionType).FirstOrDefault(x => x.ID == connectionInfoID).ConnectionType;
                enumTypeDb.ConnectionType = connectionType;

                context.EnumerationTypes.Add(enumTypeDb);
                await context.SaveChangesAsync();
                return enumTypeDb;
            }

            bool alreadyLinked = await context.ConnectionInfoEnumerationTypes
                        .AnyAsync(i => i.EnumerationTypeID == enumTypeDb.ID && i.ConnectionInfoID == connectionInfoID);

            if (alreadyLinked)
                return null;

            return enumTypeDb;
        }

        private async Task<EnumerationValue> CheckEnumerationValueToLink(EnumerationValueExternalDto enumValueDto, EnumerationType type, int connectionInfoID)
        {
            var enumValueDb = await context.EnumerationValues
                    .FirstOrDefaultAsync(i => i.ExternalId == enumValueDto.ExternalID);

            if (enumValueDb == null)
            {
                enumValueDb = mapper.Map<EnumerationValue>(enumValueDto);
                enumValueDb.EnumerationType = type;
                context.EnumerationValues.Add(enumValueDb);
                await context.SaveChangesAsync();
                return enumValueDb;
            }

            bool alreadyLinked = await context.ConnectionInfoEnumerationValues
                        .AnyAsync(i => i.EnumerationValueID == enumValueDb.ID && i.ConnectionInfoID == connectionInfoID);

            if (alreadyLinked)
                return null;

            return enumValueDb;
        }

        private async Task UpdateEnumerationObjects(ConnectionInfo connectionInfo, ConnectionInfoExternalDto connectionInfoExternalDto)
        {
            // Update types stored in connection info
            var newTypes = connectionInfoExternalDto.EnumerationTypes ?? Enumerable.Empty<EnumerationTypeExternalDto>();
            var currentEnumerationTypes = connectionInfo.EnumerationTypes.ToList();
            var typesToRemove = currentEnumerationTypes?
                .Where(x => newTypes.All(t => t.ExternalID != x.EnumerationType.ExternalId))
                .ToList();
            context.ConnectionInfoEnumerationTypes.RemoveRange(typesToRemove);

            // Update values stored in connection info
            var newValues = connectionInfoExternalDto.EnumerationTypes?
                .SelectMany(x => x.EnumerationValues)?.ToList() ?? Enumerable.Empty<EnumerationValueExternalDto>();
            var currentEnumerationValues = connectionInfo.EnumerationValues.ToList();
            var valuesToRemove = currentEnumerationValues?
                .Where(x => !newValues.Any(t =>
                    t.ExternalID == x.EnumerationValue.ExternalId))
                .ToList();
            context.ConnectionInfoEnumerationValues.RemoveRange(valuesToRemove);

            foreach (var enumType in newTypes)
            {
                var linkedType = await LinkEnumerationTypes(enumType, connectionInfo);
                if (linkedType != null)
                {
                    foreach (var enumVal in newValues)
                    {
                        await LinkEnumerationValues(enumVal, linkedType, connectionInfo);
                    }
                }
            }

            context.Update(connectionInfo);
            await context.SaveChangesAsync();
        }

        //private async Task UpdateConnectionInfo(DMContext scopedContext, ConnectionInfoExternalDto source, ConnectionInfo destination)
        //{
        //    var helper = new CryptographyHelper();
        //    foreach (var remote in source.AuthFieldValues)
        //    {
        //        var encryptedValue = helper.EncryptAes(remote.Value);
        //        var found = destination.AuthFieldValues.FirstOrDefault(d => d.Key == remote.Key);
        //        if (found != null)
        //        {
        //            found.Value = encryptedValue;
        //            continue;
        //        }

        //        destination.AuthFieldValues.Add(new AuthFieldValue
        //        {
        //            Key = remote.Key,
        //            Value = encryptedValue,
        //        });
        //    }

        //    scopedContext.Update(destination);
        //    await scopedContext.SaveChangesAsync();
        //}
        #endregion
    }
}
