using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
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
            var project = dbContext.Users.FirstOrDefault(x => x.ExternalID == source.AuthorExternalID);
            logger.LogDebug("Found project: {@Project}", project);
            return project;
        }
    }
}
