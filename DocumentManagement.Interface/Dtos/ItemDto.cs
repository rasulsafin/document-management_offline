namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ItemDto
    {
        public ID<ItemDto> ID { get; set; }

        public string Name { get; set; }

        public string ExternalID { get; set; }

        public ItemType ItemType { get; set; }
    }
}
