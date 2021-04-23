using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
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
            try
            {
                var type = await context.ConnectionTypes.FirstOrDefaultAsync(x => x.Name == typeName);

                if (type != null)
                    throw new ArgumentException("This type name is already being used");

                var connectionType = new ConnectionType { Name = typeName };
                await context.ConnectionTypes.AddAsync(connectionType);
                await context.SaveChangesAsync();
                return (ID<ConnectionTypeDto>)connectionType.ID;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Can't add new objective type with typeName = {TypeName}", typeName);
                throw;
            }
        }

        public async Task<ConnectionTypeDto> Find(ID<ConnectionTypeDto> id)
        {
            try
            {
                var dbConnectionType = await context.ConnectionTypes
                    .Include(x => x.AppProperties)
                    .Include(x => x.AuthFieldNames)
                    .FirstOrDefaultAsync(x => x.ID == (int)id);
                if (dbConnectionType == null)
                    throw new ArgumentNullException($"ConnectionType with key {id} was not found");

                return mapper.Map<ConnectionTypeDto>(dbConnectionType);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get ConnectionType {Id}", id);
                throw;
            }
        }

        public async Task<ConnectionTypeDto> Find(string name)
        {
            try
            {
                var dbConnectionType = await context.ConnectionTypes
                    .Include(x => x.AppProperties)
                    .Include(x => x.AuthFieldNames)
                    .FirstOrDefaultAsync(t => t.Name == name);

                if (dbConnectionType == null)
                    throw new ArgumentNullException($"ConnectionType with name {name} was not found");

                return mapper.Map<ConnectionTypeDto>(dbConnectionType);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get ConnectionType {Name}", name);
                throw;
            }
        }

        public async Task<IEnumerable<ConnectionTypeDto>> GetAllConnectionTypes()
        {
            try
            {
                var dbList = await context.ConnectionTypes
                    .Include(x => x.AppProperties)
                    .Include(x => x.AuthFieldNames)
                    .ToListAsync();
                return dbList.Select(t => mapper.Map<ConnectionTypeDto>(t)).ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get list of all registered connection types");
                throw;
            }
        }

        public async Task<bool> RegisterAll()
        {
            try
            {
                var listOfTypes = ConnectionCreator.GetAllConnectionTypes();

                foreach (var typeDto in listOfTypes)
                {
                    var type = await context.ConnectionTypes
                       .Include(x => x.AppProperties)
                       .Include(x => x.ObjectiveTypes)
                       .Include(x => x.AuthFieldNames)
                       .FirstOrDefaultAsync(x => x.Name == typeDto.Name);
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

                        context.ConnectionTypes.Update(type);
                    }
                    else
                    {
                        type = mapper.Map<ConnectionType>(typeDto);
                        await context.ConnectionTypes.AddAsync(type);
                    }

                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something went wrong with presented ConnectionTypes");
                throw;
            }
        }

        public async Task<bool> Remove(ID<ConnectionTypeDto> id)
        {
            try
            {
                var type = await context.ConnectionTypes.FindAsync((int)id);
                if (type == null)
                    throw new ArgumentNullException($"ConnectionType with key {id} was not found");

                context.ConnectionTypes.Remove(type);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove connection type with key {ID}", id);
                throw;
            }
        }
    }
}
