﻿namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ItemDto
    {
        public ID<ItemDto> ID { get; set; }

        public string RelativePath { get; set; }

        public ItemType ItemType { get; set; }
    }
}
