using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Synchronization;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private static readonly Dictionary<string, Task<ICollection<SynchronizingResult>>> SYNCHRONIZATIONS =
            new Dictionary<string, Task<ICollection<SynchronizingResult>>>();

        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly Synchronizer synchronizer;
        private readonly DbContextOptions<DMContext> options;

        public ConnectionService(DMContext context, IMapper mapper, DbContextOptions<DMContext> options)
        {
            this.context = context;
            this.mapper = mapper;
            this.options = options;
            synchronizer = new Synchronizer();
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

            var connection = ConnectionCreator.GetConnection(connectionInfo.ConnectionType);
            var connectionInfoDto = mapper.Map<ConnectionInfoExternalDto>(connectionInfo);

            // Connect to Remote
            var status = await connection.Connect(connectionInfoDto);

            // Update connection info
            connectionInfoDto = await connection.UpdateConnectionInfo(connectionInfoDto);
            connectionInfo = mapper.Map(connectionInfoDto, connectionInfo);
            context.Update(connectionInfo);
            await context.SaveChangesAsync();

            // Update types stored in connection info
            var newTypes = connectionInfoDto.EnumerationTypes ?? Enumerable.Empty<EnumerationTypeExternalDto>();
            var currentEnumerationTypes = connectionInfo.EnumerationTypes.ToList();
            var typesToRemove = currentEnumerationTypes?
                .Where(x => newTypes.All(t => t.ExternalID!= x.EnumerationType.ExternalId))
                .ToList();
            context.ConnectionInfoEnumerationTypes.RemoveRange(typesToRemove);

            // Update values stored in connection info
            var newValues = connectionInfoDto.EnumerationTypes?
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
            var connection = ConnectionCreator.GetConnection(connectionInfo.ConnectionType);

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

            var dm = new DMContext(options);
            IServiceCollection services = new ServiceCollection();
            services.AddTransient(x => new ObjectiveExternalDtoProjectIdResolver(dm));
            services.AddTransient(x => new ObjectiveExternalDtoObjectiveTypeResolver(dm));
            services.AddTransient(x => new ObjectiveExternalDtoObjectiveTypeIDResolver(dm));
            services.AddTransient(x => new BimElementObjectiveTypeConverter(dm));
            services.AddTransient(x => new DynamicFieldValueResolver(dm));
            services.AddTransient(x => new DynamicFieldExternalDtoValueResolver(dm));
            services.AddTransient(x => new ConnectionInfoAuthFieldValuesResolver(new CryptographyHelper()));
            services.AddTransient(x => new ObjectiveProjectIDResolver(dm));
            services.AddTransient(x => new ObjectiveExternalDtoProjectResolver(dm));
            services.AddTransient(x => new ObjectiveObjectiveTypeResolver(dm));
            services.AddTransient(x => new ConnectionInfoDtoAuthFieldValuesResolver(new CryptographyHelper()));
            services.AddAutoMapper(typeof(MappingProfile));
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var data = new SynchronizingData
            {
                Context = dm,
                Mapper = serviceProvider.GetService<IMapper>(),
                User = user,
                ProjectsFilter = x => x.Users.Any(u => u.UserID == iUserID),
                ObjectivesFilter = x => x.Project.Users.Any(u => u.UserID == iUserID),
            };

            var connection = ConnectionCreator.GetConnection(user.ConnectionInfo.ConnectionType);
            var info = mapper.Map<ConnectionInfoExternalDto>(user.ConnectionInfo);
            var id = Guid.NewGuid().ToString();
            var task = synchronizer.Synchronize(data, connection, info);
            SYNCHRONIZATIONS.Add(id, task);
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
            if (user == null)
                return null;

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
        #endregion
    }
}
