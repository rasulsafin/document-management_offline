using System.Collections.Generic;
using System.Linq;
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

namespace MRS.DocumentManagement.Services
{
    public class ConnectionTypeService : IConnectionTypeService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ILogger<ConnectionTypeService> logger;

        public ConnectionTypeService(DMContext context, IMapper mapper, ILogger<ConnectionTypeService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<ID<ConnectionTypeDto>> Add(string typeName)
        {
            logger.LogTrace("Add started with typeName = {@TypeName}", typeName);
            try
            {
                var connectionType = new ConnectionType { Name = typeName };
                await context.ConnectionTypes.AddAsync(connectionType);
                await context.SaveChangesAsync();
                return (ID<ConnectionTypeDto>)connectionType.ID;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Can't add new objective type with typeName = {TypeName}", typeName);
                throw new InvalidDataException("Can't add new objective type", ex.InnerException);
            }
        }

        public async Task<ConnectionTypeDto> Find(ID<ConnectionTypeDto> id)
        {
            logger.LogTrace("Find started with id = {ID}", id);
            var dbConnectionType = await context.ConnectionTypes
                .Include(x => x.AppProperties)
                .Include(x => x.AuthFieldNames)
                .FirstOrDefaultAsync(x => x.ID == (int)id);
            logger.LogDebug("Found connection type : {@DBConnectionType}", dbConnectionType);
            return dbConnectionType == null ? null : mapper.Map<ConnectionTypeDto>(dbConnectionType);
        }

        public async Task<ConnectionTypeDto> Find(string name)
        {
            logger.LogTrace("Find started with name = {Name}", name);
            var dbConnectionType = await context.ConnectionTypes
                .Include(x => x.AppProperties)
                .Include(x => x.AuthFieldNames)
                .FirstOrDefaultAsync(t => t.Name == name);
            logger.LogDebug("Found connection type : {@DBConnectionType}", dbConnectionType);
            return dbConnectionType == null ? null : mapper.Map<ConnectionTypeDto>(dbConnectionType);
        }

        public async Task<IEnumerable<ConnectionTypeDto>> GetAllConnectionTypes()
        {
            logger.LogTrace("GetAllConnectionTypes started");
            var dbList = await context.ConnectionTypes
                .Include(x => x.AppProperties)
                .Include(x => x.AuthFieldNames)
                .ToListAsync();
            logger.LogDebug("Found connection types : {@DBList}", dbList);
            return dbList.Select(t => mapper.Map<ConnectionTypeDto>(t)).ToList();
        }

        public async Task<bool> RegisterAll()
        {
            logger.LogTrace("RegisterAll started");
            var listOfTypes = ConnectionCreator.GetAllConnectionTypes();
            logger.LogDebug("Creator returns: {@ListOfTypes}", listOfTypes);

            try
            {
                foreach (var typeDto in listOfTypes)
                {
                    logger.LogTrace("Registration for {Name}", typeDto.Name);
                    var type = await context.ConnectionTypes
                       .Include(x => x.AppProperties)
                       .Include(x => x.ObjectiveTypes)
                       .Include(x => x.AuthFieldNames)
                       .FirstOrDefaultAsync(x => x.Name == typeDto.Name);
                    logger.LogDebug("Found connection type: {@Type}", type);
                    var update = type != null;

                    if (update)
                    {
                        var properties = type.AppProperties.ToDictionary(x => x.Key, x => x.ID);
                        var authFieldNames = type.AuthFieldNames.ToDictionary(x => x.Name, x => x.ID);

                        type = mapper.Map(typeDto, type);

                        foreach (var property in type.AppProperties)
                        {
                            property.ID = properties.TryGetValue(property.Key, out var value) ? value : 0;
                            property.ConnectionTypeID = type.ID;
                        }

                        foreach (var authFieldName in type.AuthFieldNames)
                        {
                            authFieldName.ID = authFieldNames.TryGetValue(authFieldName.Name, out var value)
                                ? value
                                : 0;
                            authFieldName.ConnectionTypeID = type.ID;
                        }

                        logger.LogDebug("Updating info for connection type: {@Type}", type);
                        context.ConnectionTypes.Update(type);
                    }
                    else
                    {
                        type = mapper.Map<ConnectionType>(typeDto);
                        logger.LogDebug("Adding info for connection type: {@Type}", type);
                        await context.ConnectionTypes.AddAsync(type);
                    }

                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Something went wrong with presented ConnectionTypes");
                throw new InvalidDataException(
                    "Something went wrong with presented ConnectionTypes",
                    ex.InnerException);
            }
        }

        public async Task<bool> Remove(ID<ConnectionTypeDto> id)
        {
            logger.LogTrace("Remove started with id = {ID}", id);
            try
            {
                var type = await context.ConnectionTypes.FindAsync((int)id);
                logger.LogDebug("Found type: {@Type}", type);
                if (type == null)
                    return false;

                context.ConnectionTypes.Remove(type);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Can't remove connection type with key {ID}", id);
                throw new InvalidDataException($"Can't remove connection type with key {id}", ex.InnerException);
            }
        }
    }
}
