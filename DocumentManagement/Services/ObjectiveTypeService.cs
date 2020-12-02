using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Services
{
    public class ObjectiveTypeService : IObjectiveTypeService
    {
        private readonly DMContext context;

        public ObjectiveTypeService(DMContext context)
        {
            this.context = context;
        }

        public async Task<ID<ObjectiveTypeDto>> Add(string typeName)
        {
            try 
            {
                var objType = new Database.Models.ObjectiveType()
                {
                    Name = typeName
                };
                context.ObjectiveTypes.Add(objType);
                await context.SaveChangesAsync();
                return (ID<ObjectiveTypeDto>)objType.ID;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException("Can't add new objective type", ex.InnerException);
            }
        }

        public async Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id)
        {
            var dbObjective = await context.ObjectiveTypes.FindAsync((int)id);
            if (dbObjective == null)
                return null;
            return new ObjectiveTypeDto() 
            {
                ID = (ID<ObjectiveTypeDto>)dbObjective.ID,
                Name = dbObjective.Name
            };
        }

        public async Task<ObjectiveTypeDto> Find(string typename)
        {
            var dbObjective = await context.ObjectiveTypes
                .FirstOrDefaultAsync(x => x.Name == typename);
            if (dbObjective == null)
                return null;
            return new ObjectiveTypeDto()
            {
                ID = (ID<ObjectiveTypeDto>)dbObjective.ID,
                Name = dbObjective.Name
            };
        }

        public async Task<IEnumerable<ObjectiveTypeDto>> GetAllObjectiveTypes()
        {
            var db = await context.ObjectiveTypes.ToListAsync();
            return db.Select(x => new ObjectiveTypeDto()
            {
                ID = (ID<ObjectiveTypeDto>)x.ID,
                Name = x.Name
            }).ToList();
        }

        public async Task<bool> Remove(ID<ObjectiveTypeDto> id)
        {
            try 
            { 
                var type = await context.ObjectiveTypes.FindAsync((int)id);
                if (type == null)
                    return false;
                context.ObjectiveTypes.Remove(type);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException($"Can't remove objective type with key {id}", ex.InnerException);
            }
        }
    }
}
