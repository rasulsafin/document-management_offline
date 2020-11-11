namespace DocumentManagement.Interface.Models
{
    public struct NewDynamicField
    {
        public NewDynamicField(string key, string type, string value)
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
