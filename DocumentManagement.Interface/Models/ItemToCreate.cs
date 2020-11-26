namespace MRS.DocumentManagement.Interface.Models
{
    public struct ItemToCreate
    {
        public ItemToCreate(string path, ItemType itemType)
        {
            Path = path;
            ItemType = itemType;
        }

        public string Path { get; }
        public ItemType ItemType { get; }
    }
}
