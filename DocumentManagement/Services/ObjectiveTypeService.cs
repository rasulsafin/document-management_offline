using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using Microsoft.EntityFrameworkCore;

namespace MRS.DocumentManagement.Services
{
    internal class ObjectiveTypeService : IObjectiveTypeService
    {
        private readonly DMContext context;

        public ObjectiveTypeService(DMContext context)
        {
            this.context = context;
        }

        public async Task<ID<ObjectiveType>> Add(string typeName)
        {
            try 
            {
                var objType = new Database.Models.ObjectiveType()
                {
                    Name = typeName
                };
                context.ObjectiveTypes.Add(objType);
                await context.SaveChangesAsync();
                return (ID<ObjectiveType>)objType.ID;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidDataException("Can't add new objective type", ex.InnerException);
            }
        }

        public async Task<ObjectiveType> Find(ID<ObjectiveType> id)
        {
            var dbObjective = await context.ObjectiveTypes.FindAsync((int)id);
            if (dbObjective == null)
                return null;
            return new ObjectiveType() 
            {
                ID = (ID<ObjectiveType>)dbObjective.ID,
                Name = dbObjective.Name
            };
        }

        public async Task<ObjectiveType> Find(string typename)
        {
            var dbObjective = await context.ObjectiveTypes
                .FirstOrDefaultAsync(x => x.Name == typename);
            if (dbObjective == null)
                return null;
            return new ObjectiveType()
            {
                ID = (ID<ObjectiveType>)dbObjective.ID,
                Name = dbObjective.Name
            };
        }

        public async Task<IEnumerable<ObjectiveType>> GetAllObjectiveTypes()
        {
            var db = await context.ObjectiveTypes.ToListAsync();
            return db.Select(x => new ObjectiveType()
            {
                ID = (ID<ObjectiveType>)x.ID,
                Name = x.Name
            }).ToList();
        }

        public async Task<bool> Remove(ID<ObjectiveType> id)
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
