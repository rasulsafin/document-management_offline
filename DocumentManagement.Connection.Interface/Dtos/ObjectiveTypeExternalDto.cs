using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ObjectiveTypeExternalDto
    {
        public string Name { get; set; }

        public string ExternalId { get; set; }

        public ICollection<DynamicFieldExternalDto> DefaultDynamicFields { get; set; }
    }
}
