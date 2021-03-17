using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ItemExternalDtoRelativePathResolver : IValueResolver<ItemExternalDto, Item, string>
    {
        public ItemExternalDtoRelativePathResolver()
        {
        }

        public string Resolve(ItemExternalDto source, Item destination, string destMember, ResolutionContext context)
        {
            return PathHelper.GetRelativePath(source.FileName, source.ItemType);
        }
    }
}
