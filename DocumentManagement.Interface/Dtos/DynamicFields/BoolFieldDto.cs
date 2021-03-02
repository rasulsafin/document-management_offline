namespace MRS.DocumentManagement.Interface.Dtos
{
    public class BoolFieldDto : IDynamicFieldDto
    {
        public ID<IDynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.BOOL; }

        public string Name { get; set; }

        public bool Value { get; set; }
    }
}
