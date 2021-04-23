using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ObjectiveObjectiveTypeResolver : IValueResolver<Objective, ObjectiveExternalDto, ObjectiveTypeExternalDto>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveObjectiveTypeResolver> logger;

        public ObjectiveObjectiveTypeResolver(DMContext dbContext, ILogger<ObjectiveObjectiveTypeResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveObjectiveTypeResolver created");
        }

        public ObjectiveTypeExternalDto Resolve(Objective source, ObjectiveExternalDto destination, ObjectiveTypeExternalDto destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var type = dbContext.ObjectiveTypes.Find(source.ObjectiveTypeID);
            logger.LogDebug("Found type: {@Type}", type);
            var objectiveTypeExternal = new ObjectiveTypeExternalDto
            {
                Name = type.Name,
                ExternalId = type.ExternalId,
            };

            return objectiveTypeExternal;
        }
    }
}
