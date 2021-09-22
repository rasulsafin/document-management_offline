using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.External;
using Brio.Docs.Integration.Dtos;

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
