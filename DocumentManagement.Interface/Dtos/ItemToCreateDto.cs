namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ItemToCreateDto
    {
        public ItemToCreateDto(string name, ItemTypeDto itemType)
        {
            Name = name;
            ItemType = itemType;
        }

        public string Name { get; }
        public ItemTypeDto ItemType { get; }
    }
}
