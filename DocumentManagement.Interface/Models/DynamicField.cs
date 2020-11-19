namespace DocumentManagement.Interface.Models
{
    public class DynamicField
    {
        public ID<DynamicField> ID { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
