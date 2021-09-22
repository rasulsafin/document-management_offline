using AutoMapper;
using Brio.Docs.Database.Models;
using Brio.Docs.Client.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ItemExternalDtoRelativePathResolver : IValueResolver<ItemExternalDto, Item, string>
    {
        private readonly ILogger<ItemExternalDtoRelativePathResolver> logger;

        public ItemExternalDtoRelativePathResolver(ILogger<ItemExternalDtoRelativePathResolver> logger)
        {
            this.logger = logger;
            logger.LogTrace("ItemExternalDtoRelativePathResolver created");
        }

        public string Resolve(ItemExternalDto source, Item destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            return PathHelper.GetRelativePath(source.FileName, source.ItemType);
        }
    }
}
