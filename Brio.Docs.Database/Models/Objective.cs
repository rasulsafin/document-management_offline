using System;
using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class Objective : ISynchronizable<Objective>
    {
        [ForbidMerge]
        public int ID { get; set; }

        [ForbidMerge]
        public int ProjectID { get; set; }

        [ForbidMerge]
        public Project Project { get; set; }

        [ForbidMerge]
        public int? ParentObjectiveID { get; set; }

        [ForbidMerge]
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

        [ForbidMerge]
        public ObjectiveType ObjectiveType { get; set; }

        [ForbidMerge]
        public ICollection<Objective> ChildrenObjectives { get; set; }

        [ForbidMerge]
        public ICollection<DynamicField> DynamicFields { get; set; }

        [ForbidMerge]
        public ICollection<ObjectiveItem> Items { get; set; }

        [ForbidMerge]
        public ICollection<BimElementObjective> BimElements { get; set; }

        [ForbidMerge]
        public string ExternalID { get; set; }

        [ForbidMerge]
        public DateTime UpdatedAt { get; set; }

        [ForbidMerge]
        public bool IsSynchronized { get; set; }

        [ForbidMerge]
        public int? SynchronizationMateID { get; set; }

        [ForbidMerge]
        public Objective SynchronizationMate { get; set; }
    }
}
