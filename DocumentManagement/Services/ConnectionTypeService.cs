﻿using System.Collections.Generic;
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

namespace MRS.DocumentManagement.Services
{
    public class ConnectionTypeService : IConnectionTypeService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;

        public ConnectionTypeService(DMContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<ID<ConnectionTypeDto>> Add(string typeName)
        {
            try
            {
                var connectionType = new Database.Models.ConnectionType { Name = typeName };
                context.ConnectionTypes.Add(connectionType);
                await context.SaveChangesAsync();
                return (ID<ConnectionTypeDto>)connectionType.ID;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException("Can't add new objective type", ex.InnerException);
            }
        }

        public async Task<ConnectionTypeDto> Find(ID<ConnectionTypeDto> id)
        {
            var dbConnectionType = await context.ConnectionTypes
                .Include(x => x.AppProperties)
                .Include(x => x.AuthFieldNames)
                .FirstOrDefaultAsync(x => x.ID == (int)id);
            return dbConnectionType == null ? null : mapper.Map<ConnectionTypeDto>(dbConnectionType);
        }

        public async Task<ConnectionTypeDto> Find(string name)
        {
            var dbConnectionType = await context.ConnectionTypes
                .Include(x => x.AppProperties)
                .Include(x => x.AuthFieldNames)
                .FirstOrDefaultAsync(t => t.Name == name);
            return dbConnectionType == null ? null : mapper.Map<ConnectionTypeDto>(dbConnectionType);
        }

        public async Task<IEnumerable<ConnectionTypeDto>> GetAllConnectionTypes()
        {
            var dbList = await context.ConnectionTypes
                .Include(x => x.AppProperties)
                .Include(x => x.AuthFieldNames)
                .ToListAsync();
            return dbList.Select(t => mapper.Map<ConnectionTypeDto>(t)).ToList();
        }

        public async Task<bool> RegisterAll()
        {
            var listOfTypes = ConnectionCreator.GetAllConnectionTypes();

            try
            {
                foreach (var typeDto in listOfTypes)
                {
                    var type = await context.ConnectionTypes
                       .Include(x => x.AppProperties)
                       .Include(x => x.ObjectiveTypes)
                       .FirstOrDefaultAsync(x => x.Name == typeDto.Name);
                    var update = type != null;

                    if (update)
                    {
                        var properties = type.AppProperties.ToDictionary(x => x.Key, x => x.ID);
                        var objectivesTypes = type.ObjectiveTypes.ToDictionary(x => x.ExternalId, x => x.ID);

                        type = mapper.Map(typeDto, type);

                        foreach (var property in type.AppProperties)
                            property.ID = properties.TryGetValue(property.Key, out var value) ? value : 0;

                        foreach (var objectiveType in type.ObjectiveTypes)
                        {
                            objectiveType.ID = objectivesTypes.TryGetValue(objectiveType.ExternalId, out var value)
                                ? value
                                : 0;
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
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException(
                    $"Something went wrong with presented ConnectionTypes",
                    ex.InnerException);
            }
        }

        public async Task<bool> Remove(ID<ConnectionTypeDto> id)
        {
            try
            {
                var type = await context.ConnectionTypes.FindAsync((int)id);
                if (type == null)
                    return false;

                context.ConnectionTypes.Remove(type);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException($"Can't remove connection type with key {id}", ex.InnerException);
            }
        }
    }
}
