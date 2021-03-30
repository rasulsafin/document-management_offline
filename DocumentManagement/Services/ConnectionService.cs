using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Synchronization;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly Synchronizer synchronizer;
        private readonly IServiceScopeFactory  serviceScopeFactory;
        private readonly RequestQuequeService processing;
        private readonly ConnectionHelper helper;

        public ConnectionService(DMContext context, IMapper mapper, IServiceScopeFactory serviceScopeFactory, RequestQuequeService processing, ConnectionHelper helper)
        {
            this.context = context;
            this.mapper = mapper;
            this.serviceScopeFactory = serviceScopeFactory;
            this.processing = processing;
            this.helper = helper;
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

        public async Task<string> Connect(ID<UserDto> userID)
        {
            var id = Guid.NewGuid().ToString();
            var scope = serviceScopeFactory.CreateScope();
            var scopedContext = scope.ServiceProvider.GetRequiredService<DMContext>();
            var scopedMapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var scopedhelper = new ConnectionHelper(scopedContext, scopedMapper);

            Progress<double> progress = new Progress<double>(v => { processing.SetProgress(v, id); });
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
            RequestQuequeService.QUEQUE.Add(id, (task.Unwrap(), 0, src));

            return await Task.FromResult(id);
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
            var connection = ConnectionCreator.GetConnection(connectionInfo.ConnectionType);

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

            var scope = serviceScopeFactory.CreateScope();
            var scopedContext = scope.ServiceProvider.GetRequiredService<DMContext>();

            var data = new SynchronizingData
            {
                Context = scopedContext,
                Mapper = scope.ServiceProvider.GetRequiredService<IMapper>(),
                User = user,
                ProjectsFilter = x => x.Users.Any(u => u.UserID == iUserID),
                ObjectivesFilter = x => x.Project.Users.Any(u => u.UserID == iUserID),
            };

            var connection = ConnectionCreator.GetConnection(user.ConnectionInfo.ConnectionType);
            var info = mapper.Map<ConnectionInfoExternalDto>(user.ConnectionInfo);

            var id = Guid.NewGuid().ToString();
            Progress<double> progress = new Progress<double>(v => { processing.SetProgress(v, id); });
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
            RequestQuequeService.QUEQUE.Add(id, (task.Unwrap(), 0, src));

            return id;
        }
    }
}
