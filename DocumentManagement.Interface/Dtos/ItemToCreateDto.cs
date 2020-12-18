namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ItemToCreateDto
    {
        public ItemToCreateDto(string name, string externalId, ItemTypeDto itemType)
        {
            Name = name;
            ExternalItemId = externalId;
            ItemType = itemType;
        }

        public string Name { get; }
        public string ExternalItemId { get; set; }
        public ItemTypeDto ItemType { get; }
    }
}
