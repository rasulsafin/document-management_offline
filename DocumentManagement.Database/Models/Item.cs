using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class Item : ISynchronizable<Item>
    { 
        public int ID { get; set; }

        public string Name { get; set; }

        public int ItemType { get; set; }

        public string ExternalID { get; set; }

        public ICollection<ProjectItem> Projects { get; set; }

        public ICollection<ObjectiveItem> Objectives { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsSynchronized { get; set; }

        public int? SynchronizationMateID { get; set; }

        public Item SynchronizationMate { get; set; }
    }
}
