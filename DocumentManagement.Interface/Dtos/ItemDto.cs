namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ItemDto
    {
        public ID<ItemDto> ID { get; set; }
        public string Path { get; set; }
        public ItemTypeDto ItemType { get; set; }
    }
}
