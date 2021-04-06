using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Utility.Factories;

namespace MRS.DocumentManagement.Utility
{
    public class ConnectionHelper
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IFactory<Type, IConnection> connectionFactory;
        private readonly ILogger<ConnectionHelper> logger;

        public ConnectionHelper(
            DMContext context,
            IMapper mapper,
            IFactory<Type, IConnection> connectionFactory,
            ILogger<ConnectionHelper> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.connectionFactory = connectionFactory;
            this.logger = logger;
        }

        internal async Task<ConnectionInfo> GetConnectionInfoFromDb(int userID)
        {
            User user = await FindUserFromDb(userID);
            if (user == null)
                throw new ArgumentNullException($"User with key {userID} was not found");

            return await GetConnectionInfoFromDb(user);
        }

        internal async Task<User> FindUserFromDb(int userID)
        {
            return await context.Users
                            .Include(x => x.ConnectionInfo)
                            .FirstOrDefaultAsync(x => x.ID == userID);
        }

        internal async Task<ConnectionInfo> GetConnectionInfoFromDb(User user)
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

        internal async Task<EnumerationType> LinkEnumerationTypes(EnumerationTypeExternalDto enumType, ConnectionInfo connectionInfo)
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

        internal async Task LinkEnumerationValues(EnumerationValueExternalDto enumVal, EnumerationType type, ConnectionInfo connectionInfo)
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

        internal async Task<EnumerationType> CheckEnumerationTypeToLink(EnumerationTypeExternalDto enumTypeDto, int connectionInfoID)
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

        internal async Task<EnumerationValue> CheckEnumerationValueToLink(EnumerationValueExternalDto enumValueDto, EnumerationType type, int connectionInfoID)
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

        internal async Task UpdateEnumerationObjects(ConnectionInfo connectionInfo, ConnectionInfoExternalDto connectionInfoExternalDto)
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

        public async Task<RequestResult> ConnectToRemote(int userID, IProgress<double> progress, CancellationToken token)
        {
            User user = await context.Users
                            .Include(x => x.ConnectionInfo)
                            .FirstOrDefaultAsync(x => x.ID == userID);
            if (user == null)
            {
                progress?.Report(1.0);
                return new RequestResult(new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = "Пользователь отсутвует в базе!", });
            }

            token.ThrowIfCancellationRequested();

            // Get connection info from user
            var connectionInfo = await GetConnectionInfoFromDb(user);
            if (connectionInfo == null)
            {
                progress?.Report(1.0);
                return new RequestResult(new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = "Подключение не найдено! (connectionInfo == null)", });
            }

            var connection = connectionFactory.Create(ConnectionCreator.GetConnection(connectionInfo.ConnectionType));
            var connectionInfoExternalDto = mapper.Map<ConnectionInfoExternalDto>(connectionInfo);

            // Connect to Remote
            var status = new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK };
            token.ThrowIfCancellationRequested();

            try
            {
                status = await connection.Connect(connectionInfoExternalDto);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't connect with info {@ConnectionInfo}", connectionInfo);
                progress?.Report(1.0);
                return new RequestResult( new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = e.Message });
            }

            // Update connection info
            connectionInfoExternalDto = await connection.UpdateConnectionInfo(connectionInfoExternalDto);
            connectionInfo = mapper.Map(connectionInfoExternalDto, connectionInfo);

            user.ExternalID = connectionInfoExternalDto.UserExternalID;

            context.Update(connectionInfo);
            await context.SaveChangesAsync();

            token.ThrowIfCancellationRequested();

            // Update types stored in connection info
            await UpdateEnumerationObjects(connectionInfo, connectionInfoExternalDto);

            token.ThrowIfCancellationRequested();

            // Update objective types stored in connection type
            foreach (var externalType in connectionInfoExternalDto.ConnectionType.ObjectiveTypes)
            {
                var dbType = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault(x => x.ExternalId == externalType.ExternalId);
                if (dbType != null)
                {
                    dbType.DefaultDynamicFields = mapper.Map<ICollection<DynamicFieldInfo>>(externalType.DefaultDynamicFields);
                    dbType.Name = externalType.Name;
                }
                else
                {
                    var newType = mapper.Map<ObjectiveType>(externalType);
                    connectionInfo.ConnectionType.ObjectiveTypes.Add(newType);
                }
            }

            context.Update(connectionInfo);
            await context.SaveChangesAsync();

            progress?.Report(1.0);

            token.ThrowIfCancellationRequested();

            return new RequestResult(status);
        }
    }
}