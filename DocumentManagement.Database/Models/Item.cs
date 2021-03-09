using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class Item : ISynchronizable<Item>
    {
        [ForbidMergeAttribute]
        public int ID { get; set; }

        public string RelativePath { get; set; }

        public int ItemType { get; set; }

        [ForbidMergeAttribute]
        public string ExternalID { get; set; }

        [ForbidMergeAttribute]
        public int? ProjectID { get; set; }

        [ForbidMergeAttribute]
        public Project Project { get; set; }

        [ForbidMergeAttribute]
        public ICollection<ObjectiveItem> Objectives { get; set; }

        [ForbidMergeAttribute]
        public DateTime UpdatedAt { get; set; }

        [ForbidMergeAttribute]
        public bool IsSynchronized { get; set; }

        [ForbidMergeAttribute]
        public int? SynchronizationMateID { get; set; }

        [ForbidMergeAttribute]
        public Item SynchronizationMate { get; set; }
    }
}
