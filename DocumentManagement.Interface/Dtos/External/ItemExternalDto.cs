using System;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ItemExternalDto
    {
        public string Name { get; set; }

        public string ExternalID { get; set; }

        public string FullPath { get; set; }

        public ItemTypeDto ItemType { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
