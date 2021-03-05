using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class DynamicField
    {
        public int ID { get; set; }

        public string ExternalID { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public int? ObjectiveID { get; set; }

        public Objective Objective { get; set; }

        public int? ParentFieldID { get; set; }

        public DynamicField ParentField { get; set; }

        public ICollection<DynamicField> ChildrenDynamicFields { get; set; }
    }
}
