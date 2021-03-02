using System.Collections.Generic;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class DynamicFieldDto : IDynamicFieldDto
    {
        public ID<IDynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.OBJECT; }

        public string Name { get; set; }

        [JsonProperty(ItemConverterType = typeof(DynamicFieldDtoConverter))]
        public ICollection<IDynamicFieldDto> Values { get; set; }
    }
}
