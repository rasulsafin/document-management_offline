using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Utils;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Connections.Bim360.Synchronization.Extensions
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
