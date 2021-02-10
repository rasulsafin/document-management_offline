using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        // TODO: Find all connection types (as libs?) and add them to db.
        // Maybe ask ConnectionCreator about it?
        // Hardcode for now.
        public async Task<bool> RegisterAll()
        {
            // Type One.
            var typeOne = new ConnectionTypeDto();
            typeOne.Name = "tdms";
            typeOne.AuthFieldNames = new List<string>() { "login", "password", "server", "db" };
            typeOne.AppProperty = new Dictionary<string, string>();
            var typeOneDb = mapper.Map<ConnectionType>(typeOne);
            await context.ConnectionTypes.AddAsync(typeOneDb);

            // Type Two.
            var typeTwo = new ConnectionTypeDto();
            typeTwo.Name = "yandexdisk";
            typeTwo.AuthFieldNames = new List<string>() { };
            typeTwo.AppProperty = new Dictionary<string, string>();
            typeTwo.AppProperty.Add("CLIENT_ID", "b1a5acbc911b4b31bc68673169f57051");
            typeTwo.AppProperty.Add("CLIENT_SECRET", "b4890ed3aa4e4a4e9e207467cd4a0f2c");
            typeTwo.AppProperty.Add("RETURN_URL", @"http://localhost:8000/oauth/");
            var typeTwoDb = mapper.Map<ConnectionType>(typeTwo);
            await context.ConnectionTypes.AddAsync(typeTwoDb);

            // Save.
            var result = await context.SaveChangesAsync();

            return result == 2;
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
