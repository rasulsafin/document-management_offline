using System;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Integration.Dtos
{
    public class ItemExternalDto
    {
        public string ExternalID { get; set; }

        public string FileName { get; set; }

        public string FullPath { get; set; }

        public ItemType ItemType { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
