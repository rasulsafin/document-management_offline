using System;
using System.Collections.Generic;
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
            user.ConnectionInfo = connectionInfo;

            await context.SaveChangesAsync();

            return mapper.Map<ID<ConnectionInfoDto>>(connectionInfo.ID);
        }

        public async Task<ConnectionStatusDto> Connect(ID<UserDto> userID)
        {
            var connectionInfo = await GetConnectionInfoFromDb((int)userID);
            var connection = GetConnection(connectionInfo);
            var connectionInfoDto = mapper.Map<ConnectionInfoDto>(connectionInfo);

            var status = await connection.Connect(connectionInfoDto);

            connectionInfoDto = await connection.UpdateConnectionInfo(connectionInfoDto);
            connectionInfo = mapper.Map(connectionInfoDto, connectionInfo);
            context.Update(connectionInfo);

            var newTypes = connectionInfoDto.EnumerationTypes ?? Enumerable.Empty<EnumerationTypeDto>();
            var currentEnumerationTypes = connectionInfo.EnumerationTypes.ToList();
            var typesToRemove = currentEnumerationTypes
                .Where(x => !newTypes.Any(t =>
                    t.ExternalId == x.EnumerationType.ExternalId))
                .ToList();
            context.ConnectionInfoEnumerationTypes.RemoveRange(typesToRemove);

            var newValues = connectionInfoDto.EnumerationTypes
                .SelectMany(x => x.EnumerationValues).ToList() ?? Enumerable.Empty<EnumerationValueDto>();
            var currentEnumerationValues = connectionInfo.EnumerationValues.ToList();
            var valuesToRemove = currentEnumerationValues
                .Where(x => !newValues.Any(t =>
                    t.ExternalId == x.EnumerationValue.ExternalId))
                .ToList();
            context.ConnectionInfoEnumerationValues.RemoveRange(valuesToRemove);

            foreach (var enumType in newTypes)
            {
                await LinkEnumerationTypes(enumType, connectionInfo);

                foreach (var enumVal in newValues)
                {
                    await LinkEnumerationValues(enumVal, connectionInfo);
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

            return await connection.GetStatus();
        }

        public async Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID)
        {
            var connectionInfo = await GetConnectionInfoFromDb((int)userID);
            var list = connectionInfo.EnumerationValues
                .Where(x => x.EnumerationValue.EnumerationTypeID == (int)enumerationTypeID)?
                .Select(x => mapper.Map<EnumerationValueDto>(x.EnumerationValue));

            return list;
        }

        private async Task<ConnectionInfo> GetConnectionInfoFromDb(int userID)
        {
            var user = await context.Users
                .Where(x => x.ID == userID)
                .Include(x => x.ConnectionInfo)
                .ThenInclude(x => x.ConnectionType)
                .ThenInclude(x => x.EnumerationTypes)
                .ThenInclude(x => x.EnumerationValues)
                .FirstOrDefaultAsync();

            var info = await context.ConnectionInfos
                .Where(x => x.ID == user.ConnectionInfoID)
                .Include(x => x.EnumerationTypes)
                .Include(x => x.EnumerationValues)
                .FirstOrDefaultAsync();

            return info;
        }

        private IConnection GetConnection(ConnectionInfo connectionInfo)
        {
            var type = mapper.Map<ConnectionTypeDto>(connectionInfo.ConnectionType);
            return ConnectionCreator.GetConnection(type);
        }

        private async Task LinkEnumerationTypes(EnumerationTypeDto enumType, ConnectionInfo connectionInfo)
        {
            var enumTypeDb = await CheckEnumerationTypeToLink(enumType, (int)connectionInfo.ID);
            if (enumTypeDb == null)
                return;

            connectionInfo.EnumerationTypes.Add(new ConnectionInfoEnumerationType
            {
                ConnectionInfoID = connectionInfo.ID,
                EnumerationTypeID = enumTypeDb.ID,
            });

            await context.SaveChangesAsync();
        }

        private async Task LinkEnumerationValues(EnumerationValueDto enumVal, ConnectionInfo connectionInfo)
        {
            var enumValueDb = await CheckEnumerationValueToLink(enumVal, (int)connectionInfo.ID);
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

        private async Task<EnumerationValue> CheckEnumerationValueToLink(EnumerationValueDto enumValueDto, int connectionInfoID)
        {
            var enumValueDb = await context.EnumerationValues
                    .FirstOrDefaultAsync(i => i.ExternalId == enumValueDto.ExternalId);

            if (enumValueDb == null)
            {
                enumValueDb = mapper.Map<EnumerationValue>(enumValueDto);
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
    }
}
