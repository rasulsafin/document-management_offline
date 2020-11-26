namespace MRS.DocumentManagement.Interface.Models
{
    public struct DynamicFieldToCreate
    {
        public DynamicFieldToCreate(string key, string type, string value)
        {
            Key = key;
            Type = type;
            Value = value;
        }

        public string Key { get; }
        public string Type { get; }
        public string Value { get; }
    }
}
