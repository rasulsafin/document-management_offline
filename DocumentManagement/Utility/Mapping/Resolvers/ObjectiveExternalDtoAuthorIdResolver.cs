using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ObjectiveExternalDtoAuthorIdResolver : IValueResolver<ObjectiveExternalDto, Objective, int?>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveExternalDtoAuthorIdResolver> logger;

        public ObjectiveExternalDtoAuthorIdResolver(DMContext dbContext, ILogger<ObjectiveExternalDtoAuthorIdResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ItemFullPathResolver created");
        }

        public int? Resolve(ObjectiveExternalDto source, Objective destination, int? destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            if (source.AuthorExternalID == null)
                return null;
            var user = dbContext.Users.FirstOrDefault(x => x.ExternalID == source.AuthorExternalID);
            logger.LogDebug("Found user: {@User}", user);
            return user?.ID;
        }
    }
}
