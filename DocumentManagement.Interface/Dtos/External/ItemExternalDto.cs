﻿namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ItemExternalDto
    {
        public string Name { get; set; }

        public string FullPath { get; set; }

        public string ExternalItemId { get; set; }

        public ItemTypeDto ItemType { get; set; }
    }
}
