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
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Utility.Factories;

namespace MRS.DocumentManagement.Services
{
    public class ItemService : IItemService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IFactory<Type, IConnection> connectionFactory;
        private readonly ILogger<ItemService> logger;
        private readonly IRequestService requestQueue;

        public ItemService(
            DMContext context,
            IMapper mapper,
            IFactory<Type, IConnection> connectionFactory,
            ILogger<ItemService> logger,
            IRequestService requestQueue)
        {
            this.context = context;
            this.mapper = mapper;
            this.connectionFactory = connectionFactory;
            this.logger = logger;
            this.requestQueue = requestQueue;
            logger.LogTrace("ItemService created");
        }

        public Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds)
        {
            logger.LogTrace("DeleteItems started with itemIds: {@ItemIds}", itemIds);

            // TODO: Delete Items from Remote Connection
            throw new NotImplementedException();
        }

        public async Task<RequestID> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
        {
            logger.LogTrace("DownloadItems started for user {@UserID} with itemIds: {@ItemIds}", userID, itemIds);
            var ids = itemIds.Select(x => (int)x).ToArray();
            var dbItems = await context.Items
                .Where(x => ids.Contains(x.ID))
                .ToListAsync();
            logger.LogDebug("Found items: {@DBItems}", dbItems);
            var project = await context.Projects
                .Where(x => x.ID == (int)dbItems.FirstOrDefault().ProjectID)
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
                .FirstOrDefaultAsync(x => x.ID == (int)userID);
            logger.LogDebug("Found user: {@User}", user);

            var connection =
                connectionFactory.Create(ConnectionCreator.GetConnection(user.ConnectionInfo.ConnectionType));
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
                    logger.LogTrace("DownloadItems task started ({ID})", id);
                    var result = await storage.DownloadFiles(project.ExternalID, data, progress, src.Token);
                    return new RequestResult(result);
                },
                TaskCreationOptions.LongRunning);
            requestQueue.AddRequest(id, task.Unwrap(), src);

            return new RequestID(id);
        }

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            logger.LogTrace("Find started with itemID: {@ItemID}", itemID);
            var dbItem = await context.Items.FindAsync((int)itemID);
            logger.LogDebug("Found dbItem: {@DbItem}", dbItem);
            return dbItem == null ? null : MapItemFromDB(dbItem);
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            logger.LogTrace("GetItems started with projectID: {@ProjectID}", projectID);
            var dbItems = (await context.Projects
                   .Include(x => x.Items)
                   .FirstOrDefaultAsync(x => x.ID == (int)projectID))?.Items
             ?? Enumerable.Empty<Item>();
            logger.LogDebug("Found dbItems: {@DbItem}", dbItems);
            return dbItems.Select(MapItemFromDB).ToList();
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            logger.LogTrace("GetItems started with objectiveID: {@ObjectiveID}", objectiveID);
            var dbItems = await context.ObjectiveItems
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .Select(x => x.Item)
                .ToListAsync();
            logger.LogDebug("Found dbItems: {@DbItem}", dbItems);
            return dbItems.Select(MapItemFromDB).ToList();
        }

        public async Task<bool> Update(ItemDto item)
        {
            logger.LogTrace("Update started with item: {@Item}", item);
            var dbItem = await context.Items.FindAsync((int)item.ID);
            logger.LogDebug("Found dbItem: {@DbItem}", dbItem);
            if (dbItem == null)
                return false;

            dbItem.ItemType = (int)item.ItemType;
            dbItem.RelativePath = item.RelativePath;
            context.Items.Update(dbItem);
            await context.SaveChangesAsync();
            return true;
        }

        private ItemDto MapItemFromDB(Item dbItem)
            => mapper.Map<ItemDto>(dbItem);
    }
}
