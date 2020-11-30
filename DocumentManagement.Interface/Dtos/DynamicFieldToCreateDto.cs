namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct DynamicFieldToCreateDto
    {
        public DynamicFieldToCreateDto(string key, string type, string value)
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
