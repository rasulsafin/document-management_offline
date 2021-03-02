namespace MRS.DocumentManagement.Interface.Dtos
{
    public class IntFieldDto : IDynamicFieldDto
    {
        public ID<IDynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.INTEGER; }

        public string Name { get; set; }

        public int Value { get; set; }
    }
}
