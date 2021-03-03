namespace MRS.DocumentManagement.Interface.Dtos
{
    public class DynamicFieldExternalDto
    {
        public string ExternalID { get; set; }

        public DynamicFieldType Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string ParentExternalID { get; set; }
    }
}
