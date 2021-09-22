using System.Linq;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Client.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ObjectiveExternalDtoAuthorResolver : IValueResolver<ObjectiveExternalDto, Objective, User>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveExternalDtoAuthorResolver> logger;

        public ObjectiveExternalDtoAuthorResolver(
            DMContext dbContext,
            ILogger<ObjectiveExternalDtoAuthorResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveExternalDtoAuthorResolver created");
        }

        public User Resolve(ObjectiveExternalDto source, Objective destination, User destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            if (source.AuthorExternalID == null)
                return null;
            var project = dbContext.Users.FirstOrDefault(x => x.ExternalID == source.AuthorExternalID);
            logger.LogDebug("Found project: {@Project}", project);
            return project;
        }
    }
}
