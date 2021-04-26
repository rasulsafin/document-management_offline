using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ObjectiveExternalDtoObjectiveTypeResolver : IValueResolver<ObjectiveExternalDto, Objective, ObjectiveType>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveExternalDtoObjectiveTypeResolver> logger;

        public ObjectiveExternalDtoObjectiveTypeResolver(DMContext dbContext, ILogger<ObjectiveExternalDtoObjectiveTypeResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveExternalDtoObjectiveTypeResolver created");
        }

        public ObjectiveType Resolve(ObjectiveExternalDto source, Objective destination, ObjectiveType destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var objectiveType = dbContext.ObjectiveTypes
               .FirstOrDefault(x => x.Name == source.ObjectiveType.Name || x.ExternalId == source.ExternalID);
            logger.LogDebug("Found objectiveType: {@ObjectiveType}", objectiveType);
            return objectiveType;
        }
    }
}
