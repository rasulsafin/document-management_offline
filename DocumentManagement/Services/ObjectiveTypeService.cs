﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Services
{
    public class ObjectiveTypeService : IObjectiveTypeService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ILogger<ObjectiveService> logger;

        public ObjectiveTypeService(DMContext context, IMapper mapper, ILogger<ObjectiveService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<ID<ObjectiveTypeDto>> Add(string typeName)
        {
            try
            {
                var objType = new Database.Models.ObjectiveType
                {
                    Name = typeName,
                };
                context.ObjectiveTypes.Add(objType);
                await context.SaveChangesAsync();
                return (ID<ObjectiveTypeDto>)objType.ID;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Can't add new objective type with typeName = {TypeName}", typeName);
                throw new InvalidDataException("Can't add new objective type", ex.InnerException);
            }
        }

        public async Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id)
        {
            var dbObjective = await context.ObjectiveTypes
                .Include(x => x.DefaultDynamicFields)
                .FirstOrDefaultAsync(x => x.ID == (int)id);
            return dbObjective == null ? null : mapper.Map<ObjectiveTypeDto>(dbObjective);
        }

        public async Task<ObjectiveTypeDto> Find(string typename)
        {
            var dbObjective = await context.ObjectiveTypes
                .Include(x => x.DefaultDynamicFields)
                .FirstOrDefaultAsync(x => x.Name == typename);
            return dbObjective == null ? null : mapper.Map<ObjectiveTypeDto>(dbObjective);
        }

        public async Task<IEnumerable<ObjectiveTypeDto>> GetObjectiveTypes(ID<ConnectionTypeDto> id)
        {
            var db = await context.ObjectiveTypes
                .Include(x => x.DefaultDynamicFields)
                .Where(x => x.ConnectionTypeID == Check((int)id))
                .ToListAsync();
            return db.Select(x => mapper.Map<ObjectiveTypeDto>(x)).ToList();
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
                logger.LogError(ex, "Can't remove objective type with key {ID}", id);
                throw new InvalidDataException($"Can't remove objective type with key {id}", ex.InnerException);
            }
        }

        private int? Check(int id) => id == -1 ? (int?)null : id;
    }
}
