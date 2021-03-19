using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class DynamicFieldInfo
    {
        public int ID { get; set; }

        public string ExternalID { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public int? ObjectiveTypeID { get; set; }

        public ObjectiveType ObjectiveType { get; set; }

        public DynamicFieldInfo ParentField { get; set; }

        public ICollection<DynamicFieldInfo> ChildrenDynamicFields { get; set; }
    }
}
