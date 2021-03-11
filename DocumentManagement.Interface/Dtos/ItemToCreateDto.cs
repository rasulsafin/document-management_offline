namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ItemToCreateDto
    {
        public string Name { get; }
        public string ExternalItemId { get; set; }
        public ItemType ItemType { get; }

        public ItemToCreateDto(string name, string externalId, ItemType itemType)
        {
            Name = name;
            ExternalItemId = externalId;
            ItemType = itemType;
        }
    }
}
