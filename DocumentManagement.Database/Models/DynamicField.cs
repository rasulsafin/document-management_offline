namespace MRS.DocumentManagement.Database.Models
{
    public class DynamicField
    {
        public int ID { get; set; }

        public int ObjectiveID { get; set; }
        public Objective Objective { get; set; }

        public string Key { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
