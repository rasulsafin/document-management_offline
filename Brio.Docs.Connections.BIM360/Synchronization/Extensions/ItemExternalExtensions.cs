using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.External;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.Synchronization.Extensions
{
    public static class ItemExternalExtensions
    {
        public static ItemExternalDto ToDto(this Item item)
            => new ()
            {
                ExternalID = item.ID,
                FileName = item.Attributes.DisplayName,
                ItemType = ItemTypeHelper.GetTypeByName(item.Attributes.DisplayName),
                UpdatedAt = item.Attributes.LastModifiedTime ?? default,
            };

        public static ItemExternalDto ToDto(this Attachment attachment)
            => new ()
            {
                ExternalID = attachment.Attributes.Urn,
                FileName = attachment.Attributes.Name,
                ItemType = ItemTypeHelper.GetTypeByName(attachment.Attributes.Name),
                UpdatedAt = attachment.Attributes.UpdatedAt ?? default,
            };
    }
}
