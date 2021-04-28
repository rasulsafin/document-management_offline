using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
{
    public class ItemFileNameResolver : IValueResolver<Item, ItemExternalDto, string>
    {
        private readonly ILogger<ItemFileNameResolver> logger;

        public ItemFileNameResolver(ILogger<ItemFileNameResolver> logger)
        {
            this.logger = logger;
            logger.LogTrace("ItemFileNameResolver created");
        }

        public string Resolve(Item source, ItemExternalDto destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            return PathHelper.GetFileName(source.RelativePath);
        }
    }
}
