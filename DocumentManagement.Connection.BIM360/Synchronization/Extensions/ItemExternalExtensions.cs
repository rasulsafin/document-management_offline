using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions
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
