using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ItemFullPathResolver : IValueResolver<Item, ItemExternalDto, string>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ItemFullPathResolver> logger;

        public ItemFullPathResolver(DMContext dbContext, ILogger<ItemFullPathResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ItemFullPathResolver created");
        }

        public string Resolve(Item source, ItemExternalDto destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var projectID = source.Project?.ID ??
                source.ProjectID ?? source.Objectives?.FirstOrDefault()?.Objective?.ProjectID ?? 0;
            logger.LogDebug("Project ID of the item = {ProjectID}", projectID);
            var project = dbContext.Projects.FirstOrDefault(x => x.ID == projectID);
            logger.LogDebug("Found project {@Project}", project);
            return project == null ? null : PathHelper.GetFullPath(project, source.RelativePath);
        }
    }
}
