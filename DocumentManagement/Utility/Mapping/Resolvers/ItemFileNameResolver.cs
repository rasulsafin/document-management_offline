using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ItemFileNameResolver : IValueResolver<Item, ItemExternalDto, string>
    {
        public ItemFileNameResolver()
        {
        }

        public string Resolve(Item source, ItemExternalDto destination, string destMember, ResolutionContext context)
        {
            return PathHelper.GetFileName(source.RelativePath);
        }
    }
}
