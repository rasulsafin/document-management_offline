using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.General.Utils.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Services
{
    public class ObjectiveTypeService : IObjectiveTypeService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ILogger<ObjectiveTypeService> logger;

        public ObjectiveTypeService(
            DMContext context,
            IMapper mapper,
            ILogger<ObjectiveTypeService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("ObjectiveTypeService created");
        }

        public async Task<ID<ObjectiveTypeDto>> Add(string typeName)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with typeName: {TypeName}", typeName);
            try
            {
                // TODO: Unique type for name+external id pair
                var objType = mapper.Map<ObjectiveType>(typeName);
                await context.ObjectiveTypes.AddAsync(objType);
                await context.SaveChangesAsync();
                return (ID<ObjectiveTypeDto>)objType.ID;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add new objective type with typeName = {TypeName}", typeName);
                throw;
            }
        }

        public async Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with id: {ID}", id);

            try
            {
                var dbObjective = await context.ObjectiveTypes
                   .Include(x => x.DefaultDynamicFields)
                   .FirstOrDefaultAsync(x => x.ID == (int)id);
                logger.LogDebug("Found objective type: {@ObjectiveType}", dbObjective);
                if (dbObjective == null)
                    throw new ArgumentNullException($"ObjectiveType with key {id} was not found");

                return mapper.Map<ObjectiveTypeDto>(dbObjective);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get ObjectiveType {Id}", id);
                throw;
            }
        }

        public async Task<ObjectiveTypeDto> Find(string typename)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with typename: {Typename}", typename);

            try
            {
                var dbObjective = await context.ObjectiveTypes
                   .Include(x => x.DefaultDynamicFields)
                   .FirstOrDefaultAsync(x => x.Name == typename);
                logger.LogDebug("Found objective type: {@ObjectiveType}", dbObjective);

                if (dbObjective == null)
                    throw new ArgumentNullException($"ObjectiveType with name {typename} was not found");

                return mapper.Map<ObjectiveTypeDto>(dbObjective);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get ObjectiveType {Typename}", typename);
                throw;
            }
        }

        public async Task<IEnumerable<ObjectiveTypeDto>> GetObjectiveTypes(ID<ConnectionTypeDto> id)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectiveTypes started with connection type id: {ID}", id);

            try
            {
                int? connectionTypeId = (int)id == -1 ? (int?)null : (int)id;

                if (connectionTypeId != null)
                {
                    var connectionType = await context.ConnectionTypes.FindAsync((int)connectionTypeId);
                    if (connectionType == null)
                        throw new ArgumentNullException($"ConnectionType with id {id} was not found");
                }

                var db = await context.ObjectiveTypes
                   .Include(x => x.DefaultDynamicFields)
                   .Where(x => x.ConnectionTypeID == connectionTypeId)
                   .ToListAsync();
                logger.LogDebug("Found objective types: {@ObjectiveTypes}", db);
                return db.Select(x => mapper.Map<ObjectiveTypeDto>(x)).ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get list of objective type from connection type {Id}", id);
                throw;
            }
        }

        public async Task<bool> Remove(ID<ObjectiveTypeDto> id)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Remove started with id: {ID}", id);
            try
            {
                var type = await context.ObjectiveTypes.FindAsync((int)id);
                logger.LogDebug("Found objective type: {@ObjectiveType}", type);
                if (type == null)
                    throw new ArgumentNullException($"ObjectiveType with id {id} was not found");

                context.ObjectiveTypes.Remove(type);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't remove objective type with key {ID}", id);
                throw;
            }
        }
    }
}
