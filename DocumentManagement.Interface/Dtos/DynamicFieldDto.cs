namespace MRS.DocumentManagement.Interface.Dtos
{
    public class DynamicFieldDto
    {
        public ID<DynamicFieldDto> ID { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
