using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class Objective : ISynchronizable<Objective>
    {
        [ForbidMergeAttribute]
        public int ID { get; set; }

        [ForbidMergeAttribute]
        public int ProjectID { get; set; }

        [ForbidMergeAttribute]
        public Project Project { get; set; }

        [ForbidMergeAttribute]
        public int? ParentObjectiveID { get; set; }

        [ForbidMergeAttribute]
        public Objective ParentObjective { get; set; }

        public int? AuthorID { get; set; }

        public User Author { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public string Title { get; set; }

        public string TitleToLower { get; set; }

        public string Description { get; set; }

        public int Status { get; set; }

        public int ObjectiveTypeID { get; set; }

        public Location Location { get; set; }

        [ForbidMergeAttribute]
        public ObjectiveType ObjectiveType { get; set; }

        [ForbidMergeAttribute]
        public ICollection<Objective> ChildrenObjectives { get; set; }

        [ForbidMergeAttribute]
        public ICollection<DynamicField> DynamicFields { get; set; }

        [ForbidMergeAttribute]
        public ICollection<ObjectiveItem> Items { get; set; }

        [ForbidMergeAttribute]
        public ICollection<BimElementObjective> BimElements { get; set; }

        [ForbidMergeAttribute]
        public string ExternalID { get; set; }

        [ForbidMergeAttribute]
        public DateTime UpdatedAt { get; set; }

        [ForbidMergeAttribute]
        public bool IsSynchronized { get; set; }

        [ForbidMergeAttribute]
        public int? SynchronizationMateID { get; set; }

        [ForbidMergeAttribute]
        public Objective SynchronizationMate { get; set; }
    }
}
