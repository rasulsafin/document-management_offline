using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class Project : ISynchronizable<Project>
    {
        [ForbidMergeAttribute]
        public int ID { get; set; }

        public string Title { get; set; }

        [ForbidMergeAttribute]
        public ICollection<Objective> Objectives { get; set; }

        [ForbidMergeAttribute]
        public ICollection<UserProject> Users { get; set; }

        [ForbidMergeAttribute]
        public ICollection<Item> Items { get; set; }

        [ForbidMergeAttribute]
        public string ExternalID { get; set; }

        [ForbidMergeAttribute]
        public DateTime UpdatedAt { get; set; }

        [ForbidMergeAttribute]
        public bool IsSynchronized { get; set; }

        [ForbidMergeAttribute]
        public int? SynchronizationMateID { get; set; }

        [ForbidMergeAttribute]
        public Project SynchronizationMate { get; set; }
    }
}
