using System.Linq;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Interface.Dtos;
using Microsoft.Extensions.Logging;
using Brio.Docs.Database.Extensions;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ObjectiveExternalDtoProjectResolver : IValueResolver<ObjectiveExternalDto, Objective, Project>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveExternalDtoProjectResolver> logger;

        public ObjectiveExternalDtoProjectResolver(
            DMContext dbContext,
            ILogger<ObjectiveExternalDtoProjectResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveExternalDtoProjectResolver created");
        }

        public Project Resolve(ObjectiveExternalDto source, Objective destination, Project destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var project = dbContext.Projects.Synchronized().FirstOrDefault(x => x.ExternalID == source.ProjectExternalID);
            logger.LogDebug("Found project: {@Project}", project);
            return project;
        }
    }
}
