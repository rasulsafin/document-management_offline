using System;

namespace MRS.DocumentManagement.Interface.Dtos
{
    public class DateFieldDto : IDynamicFieldDto
    {
        public ID<IDynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.DATE; }

        public string Name { get; set; }

        public DateTime Value { get; set; }
    }
}
