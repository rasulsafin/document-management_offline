using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class DynamicFieldExternalDto
    {
        public string ExternalID { get; set; }

        public DynamicFieldType Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public ICollection<DynamicFieldExternalDto> ChildrenDynamicFields { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
