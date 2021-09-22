using Brio.Docs.Connection.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connection.Utils;
using Brio.Docs.Interface.Dtos;

namespace Brio.Docs.Connection.Bim360.Synchronization.Extensions
{
    public static class ItemExternalExtensions
    {
        public static ItemExternalDto ToDto(this Item item)
            => new ItemExternalDto
            {
                ExternalID = item.ID,
                FileName = item.Attributes.DisplayName,
                ItemType = ItemTypeHelper.GetTypeByName(item.Attributes.DisplayName),
                UpdatedAt = item.Attributes.LastModifiedTime ?? default,
            };
    }
}
