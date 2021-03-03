using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions
{
    public static class ItemExternalExtensions
    {
        public static ItemExternalDto ToDto(this Item item)
            => new ItemExternalDto
            {
                ExternalID = item.ID,
                Name = item.Attributes.Name,
                ItemType = ItemTypeHelper.GetTypeByName(item.Attributes.Name),
                UpdatedAt = item.Attributes.LastModifiedTime ?? default,
            };
    }
}
