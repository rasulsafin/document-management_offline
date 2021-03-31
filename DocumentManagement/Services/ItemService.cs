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
using MRS.DocumentManagement.Utility.Factories;

namespace MRS.DocumentManagement.Services
{
    public class ItemService : IItemService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly IFactory<Type, IConnection> connectionFactory;

        public ItemService(DMContext context, IMapper mapper, IFactory<Type, IConnection> connectionFactory)
        {
            this.context = context;
            this.mapper = mapper;
            this.connectionFactory = connectionFactory;
        }

        public Task<bool> DeleteItems(IEnumerable<ID<ItemDto>> itemIds)
        {
            // TODO: Delete Items from Remote Connection
            throw new NotImplementedException();
        }

        public async Task<bool> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds)
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

            var connection =
                connectionFactory.Create(ConnectionCreator.GetConnection(user.ConnectionInfo.ConnectionType));
            var info = mapper.Map<ConnectionInfoExternalDto>(user.ConnectionInfo);
            var storage = await connection.GetStorage(info);

            return await storage.DownloadFiles(project.ExternalID, dbItems.Select(x => mapper.Map<ItemExternalDto>(x)));
        }

        public async Task<ItemDto> Find(ID<ItemDto> itemID)
        {
            var dbItem = await context.Items.FindAsync((int)itemID);
            return dbItem == null ? null : MapItemFromDB(dbItem);
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID)
        {
            var dbItems = (await context.Projects
                   .Include(x => x.Items)
                   .FirstOrDefaultAsync(x => x.ID == (int)projectID))?.Items
             ?? Enumerable.Empty<Item>();
            return dbItems.Select(MapItemFromDB).ToList();
        }

        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID)
        {
            var dbItems = await context.ObjectiveItems
                .Where(x => x.ObjectiveID == (int)objectiveID)
                .Select(x => x.Item)
                .ToListAsync();
            return dbItems.Select(MapItemFromDB).ToList();
        }

        public async Task<bool> Update(ItemDto item)
        {
            var dbItem = await context.Items.FindAsync((int)item.ID);
            if (dbItem == null)
                return false;

            dbItem.ItemType = (int)item.ItemType;
            dbItem.RelativePath = item.RelativePath;
            context.Items.Update(dbItem);
            await context.SaveChangesAsync();
            return true;
        }

        private ItemDto MapItemFromDB(Database.Models.Item dbItem)
            => mapper.Map<ItemDto>(dbItem);
    }
}
