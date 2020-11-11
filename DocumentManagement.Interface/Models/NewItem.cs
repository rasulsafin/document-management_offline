namespace DocumentManagement.Interface.Models
{
    public struct NewItem
    {
        public NewItem(string path, ItemType itemType)
        {
            Path = path;
            ItemType = itemType;
        }

        public string Path { get; }
        public ItemType ItemType { get; }
    }
}
