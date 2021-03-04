namespace MRS.DocumentManagement.Interface.Dtos
{
    public class EnumerationFieldDto : IDynamicFieldDto<EnumerationValueDto>
    {
        public ID<IDynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.ENUM; }

        public string Name { get; set; }

        public EnumerationValueDto Value { get; set; }

        public EnumerationTypeDto EnumerationType { get; set; }
    }
}
