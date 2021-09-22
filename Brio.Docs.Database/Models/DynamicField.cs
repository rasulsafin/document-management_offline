using System;
using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class DynamicField : ISynchronizable<DynamicField>, IDynamicField
    {
        [ForbidMerge]
        public int ID { get; set; }

        [ForbidMerge]
        public string ExternalID { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        [ForbidMerge]
        public int? ObjectiveID { get; set; }

        [ForbidMerge]
        public Objective Objective { get; set; }

        [ForbidMerge]
        public int? ParentFieldID { get; set; }

        [ForbidMerge]
        public DynamicField ParentField { get; set; }

        [ForbidMerge]
        public ICollection<DynamicField> ChildrenDynamicFields { get; set; }

        [ForbidMerge]
        public DateTime UpdatedAt { get; set; }

        [ForbidMerge]
        public int? ConnectionInfoID { get; set; }

        [ForbidMerge]
        public ConnectionInfo ConnectionInfo { get; set; }

        [ForbidMerge]
        public bool IsSynchronized { get; set; }

        [ForbidMerge]
        public int? SynchronizationMateID { get; set; }

        [ForbidMerge]
        public DynamicField SynchronizationMate { get; set; }
    }
}
