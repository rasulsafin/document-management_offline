using System;
using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class Project : ISynchronizable<Project>
    {
        [ForbidMerge]
        public int ID { get; set; }

        public string Title { get; set; }

        [ForbidMerge]
        public ICollection<Objective> Objectives { get; set; }

        [ForbidMerge]
        public ICollection<UserProject> Users { get; set; }

        [ForbidMerge]
        public ICollection<Item> Items { get; set; }

        [ForbidMerge]
        public string ExternalID { get; set; }

        [ForbidMerge]
        public DateTime UpdatedAt { get; set; }

        [ForbidMerge]
        public bool IsSynchronized { get; set; }

        [ForbidMerge]
        public int? SynchronizationMateID { get; set; }

        [ForbidMerge]
        public Project SynchronizationMate { get; set; }
    }
}
