using System.Collections.Generic;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class ObjectiveTypeDto
    {
        public ID<ObjectiveTypeDto> ID { get; set; }

        public string Name { get; set; }

        public ICollection<DynamicFieldDto> DefaultDynamicFields { get; set; }
    }
}
