using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;

        public ConnectionService(DMContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
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
                return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.Error, Message = "Пользователь отсутвует в базе!", };

            // Get connection info from user
            var connectionInfo = await GetConnectionInfoFromDb(user);
            if (connectionInfo == null)
                return new ConnectionStatusDto() { Status = RemoteConnectionStatusDto.Error, Message = "Подключение не найдено! (connectionInfo == null)", };

            var connection = GetConnection(connectionInfo);
            var connectionInfoDto = mapper.Map<ConnectionInfoDto>(connectionInfo);

            // Connect to Remote
            var status = await connection.Connect(connectionInfoDto);

            // Update connection info
            connectionInfoDto = await connection.UpdateConnectionInfo(connectionInfoDto);
            connectionInfo = mapper.Map(connectionInfoDto, connectionInfo);
            context.Update(connectionInfo);
            await context.SaveChangesAsync();

            // Update types stored in connection info
            var newTypes = connectionInfoDto.EnumerationTypes ?? Enumerable.Empty<EnumerationTypeDto>();
            var currentEnumerationTypes = connectionInfo.EnumerationTypes.ToList();
            var typesToRemove = currentEnumerationTypes?
                .Where(x => newTypes.All(t => t.ExternalId != x.EnumerationType.ExternalId))
                .ToList();
            context.ConnectionInfoEnumerationTypes.RemoveRange(typesToRemove);

            // Update values stored in connection info
            var newValues = connectionInfoDto.EnumerationTypes?
                .SelectMany(x => x.EnumerationValues)?.ToList() ?? Enumerable.Empty<EnumerationValueDto>();
            var currentEnumerationValues = connectionInfo.EnumerationValues.ToList();
            var valuesToRemove = currentEnumerationValues?
                .Where(x => !newValues.Any(t =>
                    t.ExternalId == x.EnumerationValue.ExternalId))
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
            var connection = GetConnection(connectionInfo);

            return await connection.GetStatus(mapper.Map<ConnectionInfoDto>(connectionInfo));
        }

        public async Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
        {
            var connectionInfo = await GetConnectionInfoFromDb((int)userID);
            var list = connectionInfo.EnumerationValues
                .Where(x => x.EnumerationValue.EnumerationTypeID == (int)enumerationTypeID)?
                .Select(x => mapper.Map<EnumerationValueDto>(x.EnumerationValue));

            return list;
        }

        #region private method
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
            var info = await context.ConnectionInfos
                .Include(x => x.ConnectionType)
                    .ThenInclude(x => x.AppProperties)
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

        private IConnection GetConnection(ConnectionInfo connectionInfo)
        {
            var type = mapper.Map<ConnectionTypeDto>(connectionInfo.ConnectionType);
            return ConnectionCreator.GetConnection(type);
        }

        private async Task<EnumerationType> LinkEnumerationTypes(EnumerationTypeDto enumType, ConnectionInfo connectionInfo)
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

        private async Task LinkEnumerationValues(EnumerationValueDto enumVal, EnumerationType type, ConnectionInfo connectionInfo)
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

        private async Task<EnumerationType> CheckEnumerationTypeToLink(EnumerationTypeDto enumTypeDto, int connectionInfoID)
        {
            var enumTypeDb = await context.EnumerationTypes
                    .FirstOrDefaultAsync(i => i.ExternalId == enumTypeDto.ExternalId);

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

        private async Task<EnumerationValue> CheckEnumerationValueToLink(EnumerationValueDto enumValueDto, EnumerationType type, int connectionInfoID)
        {
            var enumValueDb = await context.EnumerationValues
                    .FirstOrDefaultAsync(i => i.ExternalId == enumValueDto.ExternalId);

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
        #endregion
    }
}
