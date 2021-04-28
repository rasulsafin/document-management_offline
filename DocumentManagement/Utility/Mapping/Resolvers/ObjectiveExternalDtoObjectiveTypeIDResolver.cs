using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ObjectiveExternalDtoObjectiveTypeIdResolver : IValueResolver<ObjectiveExternalDto, Objective, int>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveExternalDtoObjectiveTypeIdResolver> logger;

        public ObjectiveExternalDtoObjectiveTypeIdResolver(
            DMContext dbContext,
            ILogger<ObjectiveExternalDtoObjectiveTypeIdResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveExternalDtoAuthorResolver created");
        }

        public int Resolve(ObjectiveExternalDto source, Objective destination, int destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var objectiveTypeID = dbContext.ObjectiveTypes.FirstOrDefault(x => x.ExternalId == source.ObjectiveType.ExternalId).ID;
            logger.LogDebug("Found objectiveTypeID: {@ObjectiveTypeID}", objectiveTypeID);
            return objectiveTypeID;
        }
    }
}
