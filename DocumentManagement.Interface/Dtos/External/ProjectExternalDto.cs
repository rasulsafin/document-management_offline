using System;
using System.Collections.Generic;
using System.Text;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ProjectExternalDto
    {
        public string ExternalID { get; set; }

        public string Title { get; set; }

        public IEnumerable<ItemExternalDto> Items { get; set; }
    }
}
