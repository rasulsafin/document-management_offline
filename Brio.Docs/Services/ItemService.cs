using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Connection;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.General.Utils.Factories;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using Brio.Docs.Interface.Exceptions;
using Brio.Docs.Interface.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Brio.Docs.General.Utils.Extensions;
using Brio.Docs.Utility.Extensions;

namespace Brio.Docs.Services
{
    public class ItemService : IItemService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IFactory<IServiceScope, Type, IConnection> connectionFactory;
        private readonly IRequestService requestQueue;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<ItemService> logger;

        public ItemService(
            DMContext context,
            IMapper mapper,
            IFactory<IServiceScope, Type, IConnection> connectionFactory,
            IRequestService requestQueue,
            IServiceScopeFactory scopeFactory,
            ILogger<ItemService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.connectionFactory = connectionFactory;
            this.requestQueue = requestQueue;
            this.scopeFactory = scopeFactory;
            this.logger = logger;
            logger.LogTrace("ItemService created");
        }

        public Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("DeleteItems started with itemIds: {@ItemIds}", itemIds);

            // TODO: Delete Items from Remote Connection
            throw new NotImplementedException();
        }

        public async Task<RequestID> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("DownloadItems started for user {@UserID} with itemIds: {@ItemIds}", userID, itemIds);
            try
            {
                var ids = itemIds.Select(x => (int)x).ToArray();
                var dbItems = await context.Items
                    .Where(x => ids.Contains(x.ID))
                    .ToListAsync();
                logger.LogDebug("Found items: {@DBItems}", dbItems);

                var projectID = dbItems.FirstOrDefault()?.ProjectID ?? -1;
                var project = await context.Projects
                    .Where(x => x.ID == projectID)
                    .FirstOrDefaultAsync();
                logger.LogDebug("Found project: {@Project}", project);

                var user = await context.Users
                    .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.ConnectionType)
                    .ThenInclude(x => x.AppProperties)
                    .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.ConnectionType)
                    .ThenInclude(x => x.AuthFieldNames)
                    .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.AuthFieldValues)
                    .FindOrThrowAsync(x => x.ID, (int)userID);
                logger.LogDebug("Found user: {@User}", user);

                var scope = scopeFactory.CreateScope();
                var connection =
                    connectionFactory.Create(scope, ConnectionCreator.GetConnection(user.ConnectionInfo.ConnectionType));
                var info = mapper.Map<ConnectionInfoExternalDto>(user.ConnectionInfo);
                logger.LogTrace("Mapped info: {@Info}", info);
                var storage = await connection.GetStorage(info);

                var id = Guid.NewGuid().ToString();
                Progress<double> progress = new Progress<double>(v => { requestQueue.SetProgress(v, id); });
                var data = dbItems.Select(x => mapper.Map<ItemExternalDto>(x)).ToList();
                var src = new CancellationTokenSource();

                var task = Task.Factory.StartNew(
                    async () =>
                    {
                        try
                        {
                            logger.LogTrace("DownloadItems task started ({ID})", id);
                            var result = await storage.DownloadFiles(project?.ExternalID, data, progress, src.Token);
                            return new RequestResult(result);
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
                logger.LogError(ex, "Can't download items {@ItemIds} with user key {UserID}", itemIds, userID);
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with itemID: {@ItemID}", itemID);
            try
            {
                var dbItem = await context.Items.FindOrThrowAsync((int)itemID);
                logger.LogDebug("Found dbItem: {@DbItem}", dbItem);
                return mapper.Map<ItemDto>(dbItem);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't get item with key {ItemID}", itemID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            logger.LogTrace("GetItems started with projectID: {@ProjectID}", projectID);
            try
            {
                await context.Projects.FindOrThrowAsync((int)projectID);
                var dbItems = (await context.Projects
                       .Include(x => x.Items)
                       .FirstOrDefaultAsync(x => x.ID == (int)projectID))?.Items
                 ?? Enumerable.Empty<Item>();
                logger.LogDebug("Found dbItems: {@DbItem}", dbItems);
                return dbItems.Select(x => mapper.Map<ItemDto>(x)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find items by project key {@ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetItems started with objectiveID: {@ObjectiveID}", objectiveID);
            try
            {
                await context.Objectives.FindOrThrowAsync((int)objectiveID);
                var dbItems = await context.ObjectiveItems
                    .Where(x => x.ObjectiveID == (int)objectiveID)
                    .Select(x => x.Item)
                    .ToListAsync();
                logger.LogDebug("Found dbItems: {@DbItem}", dbItems);
                return dbItems.Select(x => mapper.Map<ItemDto>(x)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find items by objective key {@ObjectiveID}", objectiveID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        public async Task<bool> Update(ItemDto item)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with item: {@Item}", item);
            try
            {
                var dbItem = await context.Items.FindOrThrowAsync((int)item.ID);
                logger.LogDebug("Found dbItem: {@DbItem}", dbItem);

                dbItem.ItemType = (int)item.ItemType;
                dbItem.RelativePath = item.RelativePath;
                context.Items.Update(dbItem);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't update item {@Item}", item);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }
    }
}
