using AutoMapper;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility.Mapping.Resolvers
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
