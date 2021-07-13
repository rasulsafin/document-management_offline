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
using MRS.DocumentManagement.General.Utils.Extensions;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Synchronization;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Utility.Extensions;

namespace MRS.DocumentManagement.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<SynchronizationService> logger;
        private readonly IFactory<IServiceScope, Type, IConnection> connectionScopedFactory;
        private readonly IFactory<IServiceScope, Synchronizer> synchronizerFactory;
        private readonly IRequestService requestQueue;

        public SynchronizationService(
            DMContext context,
            IMapper mapper,
            IRequestService requestQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<SynchronizationService> logger,
            IFactory<IServiceScope, Type, IConnection> connectionScopedFactory,
            IFactory<IServiceScope, Synchronizer> synchronizerFactory)
        {
            this.context = context;
            this.mapper = mapper;
            this.serviceScopeFactory = serviceScopeFactory;
            this.requestQueue = requestQueue;
            this.logger = logger;
            this.connectionScopedFactory = connectionScopedFactory;
            this.synchronizerFactory = synchronizerFactory;

            logger.LogTrace("SynchronizationService created");
        }

        public async Task<RequestID> Synchronize(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogInformation("Synchronize started for user: {UserID}", userID);
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
                   .FindOrThrowAsync(x => x.ID, iUserID);

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
                            return new RequestResult(
                                synchronizationResult.Count == 0,
                                synchronizationResult.FirstOrDefault()?.Exception.ConvertToBase());
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't start the synchronizing process with user id {UserID}", userID);
                throw;
            }
        }

        public async Task<IEnumerable<DateTime>> GetSynchronizationDates(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogInformation("GetSynchronizationsDates started for user: {UserID}", userID);
            try
            {
                var user = await context.FindOrThrowAsync<User>((int)userID);
                logger.LogDebug("Found user: {@User}", user);
                var synchronizations = await context.Synchronizations.Where(x => x.UserID == user.ID)
                   .Select(x => x.Date)
                   .OrderByDescending(x => x)
                   .ToListAsync();
                logger.LogDebug("Found user {UserID} synchronizations: {@Synchronizations}", userID, synchronizations);
                return synchronizations;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get synchronizations for user: {UserID}", userID);
                throw;
            }
        }

        public async Task<bool> RemoveLastSynchronizationDate(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogInformation("RemoveLastSynchronizationDate started for user: {UserID}", userID);

            try
            {
                var user = await context.FindOrThrowAsync<User>((int)userID);
                logger.LogDebug("Found user: {@User}", user);
                var synchronization = await context.Synchronizations.Where(x => x.UserID == user.ID)
                   .OrderByDescending(x => x)
                   .FirstAsync();
                logger.LogDebug("Found synchronization: {@Synchronization}", synchronization);
                context.Synchronizations.Remove(synchronization);
                await context.SaveChangesAsync();
                logger.LogInformation(
                    "Synchronization date {@Synchronization} for user {UserID} removed",
                    synchronization,
                    userID);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove last synchronization date for user: {UserID}", userID);
                throw;
            }
        }

        public async Task<bool> RemoveAllSynchronizationDates(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogInformation("RemoveAllSynchronizationDate started for user: {UserID}", userID);

            try
            {
                var user = await context.Users.Include(x => x.Synchronizations)
                   .FindOrThrowAsync(x => x.ID, (int)userID);
                logger.LogDebug("Found user: {@User}", user);
                user.Synchronizations = new List<Database.Models.Synchronization>();
                context.Update(user);
                await context.SaveChangesAsync();
                logger.LogInformation("Synchronization dates for user {UserID} removed", userID);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove synchronization dates for user: {UserID}", userID);
                throw;
            }
        }
    }
}
