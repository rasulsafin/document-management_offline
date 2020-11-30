namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct ItemToCreateDto
    {
        public ItemToCreateDto(string path, ItemTypeDto itemType)
        {
            Path = path;
            ItemType = itemType;
        }

        public string Path { get; }
        public ItemTypeDto ItemType { get; }
    }
}
