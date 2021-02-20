namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ItemDto
    {
        public ID<ItemDto> ID { get; set; }

        public string Name { get; set; }

        public string ExternalID { get; set; }

        public ItemTypeDto ItemType { get; set; }
    }
}
