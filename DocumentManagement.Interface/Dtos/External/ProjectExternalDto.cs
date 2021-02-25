using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ProjectExternalDto
    {
        public string ExternalID { get; set; }

        public string Title { get; set; }

        public IEnumerable<ItemExternalDto> Items { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
