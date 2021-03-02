namespace MRS.DocumentManagement.Interface.Dtos
{
    public class FloatFieldDto : IDynamicFieldDto
    {
        public ID<IDynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.FLOAT; }

        public string Name { get; set; }

        public float Value { get; set; }
    }
}
