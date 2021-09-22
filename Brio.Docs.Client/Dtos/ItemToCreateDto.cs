namespace Brio.Docs.Interface.Dtos
{
    public struct ItemToCreateDto
    {
        public string RelativePath { get; }
        public string ExternalItemId { get; set; }
        public ItemType ItemType { get; }

        public ItemToCreateDto(string relativePath, string externalId, ItemType itemType)
        {
            RelativePath = relativePath;
            ExternalItemId = externalId;
            ItemType = itemType;
        }
    }
}
