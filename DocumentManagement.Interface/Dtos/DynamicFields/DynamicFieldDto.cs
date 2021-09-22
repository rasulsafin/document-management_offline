using System.Collections.Generic;

namespace Brio.Docs.Interface.Dtos
{
    public class DynamicFieldDto
    {
        public ID<DynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

    }
}
