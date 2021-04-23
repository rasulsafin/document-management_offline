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
using MRS.DocumentManagement.Utility.Factories;

namespace MRS.DocumentManagement.Services
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
        }

        public Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds)
        {
            // TODO: Delete Items from Remote Connection
            throw new NotImplementedException();
        }

        public async Task<RequestID> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            try
            {
                var ids = itemIds.Select(x => (int)x).ToArray();
                var dbItems = await context.Items
                    .Where(x => ids.Contains(x.ID))
                    .ToListAsync();
                var mapped = dbItems.Select(x => mapper.Map<ItemExternalDto>(x)).ToList();
                var project = await context.Projects
                    .Where(x => x.ID == (int)dbItems.FirstOrDefault().ProjectID)
                    .FirstOrDefaultAsync();

                var user = await context.Users
                    .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.ConnectionType)
                    .ThenInclude(x => x.AppProperties)
                    .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.ConnectionType)
                    .ThenInclude(x => x.AuthFieldNames)
                    .Include(x => x.ConnectionInfo)
                    .ThenInclude(x => x.AuthFieldValues)
                    .FirstOrDefaultAsync(x => x.ID == (int)userID);

                if (user == null)
                    throw new ArgumentNullException($"User with key {userID} was not found");

                var scope = scopeFactory.CreateScope();
                var connection =
                    connectionFactory.Create(scope, ConnectionCreator.GetConnection(user.ConnectionInfo.ConnectionType));
                var info = mapper.Map<ConnectionInfoExternalDto>(user.ConnectionInfo);
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
                            var result = await storage.DownloadFiles(project.ExternalID, data, progress, src.Token);
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
            catch (Exception e)
            {
                logger.LogError(e, "Can't download items {@ItemIds} with user key {UserID}", itemIds, userID);
                throw;
            }
        }

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            try
            {
                var dbItem = await context.Items.FindAsync((int)itemID);
                if (dbItem == null)
                    throw new ArgumentNullException($"Item with id {itemID} was not found");
                return mapper.Map<ItemDto>(dbItem);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get item with key {ItemID}", itemID);
                throw;
            }
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            try
            {
                var dbProject = await context.Projects.FindAsync((int)projectID);
                if (dbProject == null)
                    throw new ArgumentNullException($"Project with id {projectID} was not found");

                var dbItems = (await context.Projects
                       .Include(x => x.Items)
                       .FirstOrDefaultAsync(x => x.ID == (int)projectID))?.Items
                 ?? Enumerable.Empty<Item>();
                return dbItems.Select(x => mapper.Map<ItemDto>(x)).ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't find items by project key {@ProjectID}", projectID);
                throw;
            }
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            try
            {
                var dbObjective = await context.Objectives.FindAsync((int)objectiveID);
                if (dbObjective == null)
                    throw new ArgumentNullException($"Objective with id {objectiveID} was not found");

                var dbItems = await context.ObjectiveItems
                    .Where(x => x.ObjectiveID == (int)objectiveID)
                    .Select(x => x.Item)
                    .ToListAsync();
                return dbItems.Select(x => mapper.Map<ItemDto>(x)).ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't find items by objective key {@ObjectiveID}", objectiveID);
                throw;
            }
        }

        public async Task<bool> Update(ItemDto item)
        {
            try
            {
                var dbItem = await context.Items.FindAsync((int)item.ID);
                if (dbItem == null)
                    throw new ArgumentNullException($"Item {item} was not found");

                dbItem.ItemType = (int)item.ItemType;
                dbItem.RelativePath = item.RelativePath;
                context.Items.Update(dbItem);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't update item {@Item}", item);
                throw;
            }
        }
    }
}
