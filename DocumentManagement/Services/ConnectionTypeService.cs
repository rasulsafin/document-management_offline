using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Services
{
    public class ConnectionTypeService : IConnectionTypeService
    {
        private readonly DMContext context;

        public ConnectionTypeService(DMContext context) => this.context = context;

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
            if (dbConnectionType == null)
                return null;

            return new ConnectionTypeDto { ID = (ID<ConnectionTypeDto>)dbConnectionType.ID, Name = dbConnectionType.Name };
        }

        public async Task<ConnectionTypeDto> Find(string name)
        {
            var dbConnectionType = await context.ConnectionTypes.FirstOrDefaultAsync(t => t.Name == name);
            if (dbConnectionType == null)
                return null;

            return new ConnectionTypeDto { ID = (ID<ConnectionTypeDto>)dbConnectionType.ID, Name = dbConnectionType.Name };
        }

        public async Task<IEnumerable<ConnectionTypeDto>> GetAllConnectionTypes()
        {
            var db = await context.ConnectionTypes.ToListAsync();

            return db.Select(t => new ConnectionTypeDto()
            {
                ID = (ID<ConnectionTypeDto>)t.ID,
                Name = t.Name,
            }).ToList();
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
