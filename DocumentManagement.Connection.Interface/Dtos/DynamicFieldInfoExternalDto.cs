using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class DynamicFieldInfoExternalDto
    {
        public string ExternalID { get; set; }

        public DynamicFieldType Type { get; set; }

        public string Name { get; set; }

        public ICollection<DynamicFieldInfoExternalDto> ChildrenDynamicFields { get; set; }
    }
}
