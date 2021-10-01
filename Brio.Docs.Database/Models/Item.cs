using System;
using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class Item : ISynchronizable<Item>
    {
        [ForbidMerge]
        public int ID { get; set; }

        public string RelativePath { get; set; }

        public int ItemType { get; set; }

        [ForbidMerge]
        public string ExternalID { get; set; }

        [ForbidMerge]
        public int? ProjectID { get; set; }

        [ForbidMerge]
        public Project Project { get; set; }

        [ForbidMerge]
        public ICollection<ObjectiveItem> Objectives { get; set; }

        [ForbidMerge]
        public DateTime UpdatedAt { get; set; }

        [ForbidMerge]
        public bool IsSynchronized { get; set; }

        [ForbidMerge]
        public int? SynchronizationMateID { get; set; }

        [ForbidMerge]
        public Item SynchronizationMate { get; set; }
    }
}
