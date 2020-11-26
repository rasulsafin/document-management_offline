namespace MRS.DocumentManagement.Interface.Models
{
    public class Item
    {
        public ID<Item> ID { get; set; }
        public string Path { get; set; }
        public ItemType ItemType { get; set; }
    }
}
