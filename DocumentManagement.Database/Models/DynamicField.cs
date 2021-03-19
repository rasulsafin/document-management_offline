using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class DynamicField : ISynchronizable<DynamicField>
    {
        [ForbidMergeAttribute]
        public int ID { get; set; }

        [ForbidMergeAttribute]
        public string ExternalID { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        [ForbidMergeAttribute]
        public int? ObjectiveID { get; set; }

        [ForbidMergeAttribute]
        public Objective Objective { get; set; }

        [ForbidMergeAttribute]
        public int? ParentFieldID { get; set; }

        [ForbidMergeAttribute]
        public DynamicField ParentField { get; set; }

        [ForbidMergeAttribute]
        public ICollection<DynamicField> ChildrenDynamicFields { get; set; }

        [ForbidMergeAttribute]
        public DateTime UpdatedAt { get; set; }

        [ForbidMergeAttribute]
        public bool IsSynchronized { get; set; }

        [ForbidMergeAttribute]
        public int? SynchronizationMateID { get; set; }

        [ForbidMergeAttribute]
        public DynamicField SynchronizationMate { get; set; }
    }
}
