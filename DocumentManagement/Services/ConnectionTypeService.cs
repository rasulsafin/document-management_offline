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
            var dbConnectionType = await context.ConnectionTypes.FindAsync((int)id);
            return dbConnectionType == null ? null : mapper.Map<ConnectionTypeDto>(dbConnectionType);
        }

        public async Task<ConnectionTypeDto> Find(string name)
        {
            var dbConnectionType = await context.ConnectionTypes.FirstOrDefaultAsync(t => t.Name == name);
            return dbConnectionType == null ? null : mapper.Map<ConnectionTypeDto>(dbConnectionType);
        }

        public async Task<IEnumerable<ConnectionTypeDto>> GetAllConnectionTypes()
        {
            var dbList = await context.ConnectionTypes.ToListAsync();
            return dbList.Select(t => mapper.Map<ConnectionTypeDto>(t)).ToList();
        }

        public async Task<bool> RegisterAll()
        {
            var listOfTypes = ConnectionCreator.GetAllConnectionTypes();

            try
            {
                foreach (var type in listOfTypes)
                {
                    var typeDb = mapper.Map<ConnectionType>(type);
                    var typeFromDb = await context.ConnectionTypes.FirstOrDefaultAsync(x => x.Name == type.Name);

                    // TODO: Update if exists?
                    if (typeFromDb == default)
                        await context.ConnectionTypes.AddAsync(typeDb);

                    await context.SaveChangesAsync();

                    if (type.EnumerationTypes == null)
                        continue;

                    foreach (var enumTypeDto in type.EnumerationTypes)
                    {
                        var enumType = await context.EnumerationTypes.FirstOrDefaultAsync(x => x.ExternalId == enumTypeDto.ExternalId);
                        enumType.ConnectionType = typeDb;
                        await context.SaveChangesAsync();

                        if (enumTypeDto.EnumerationValues == null)
                            enumTypeDto.EnumerationValues = new List<EnumerationValueDto>();

                        foreach (var enumValueDto in enumTypeDto.EnumerationValues)
                        {
                            var enumValue = mapper.Map<EnumerationValue>(enumValueDto);
                            enumValue.EnumerationTypeID = enumType.ID;

                            // TODO: Add or Update.
                            context.EnumerationValues.Add(enumValue);
                        }
                    }
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException($"Something went wrong with presented ConnectionTypes", ex.InnerException);
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
