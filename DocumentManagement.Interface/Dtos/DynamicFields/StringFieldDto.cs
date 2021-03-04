namespace MRS.DocumentManagement.Interface.Dtos
{
    public class StringFieldDto : IDynamicFieldDto<string>
    {
        public ID<IDynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.STRING; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}