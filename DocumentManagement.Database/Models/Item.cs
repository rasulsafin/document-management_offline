using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class Item
    { 
        public int ID { get; set; }

        public string Name { get; set; }
        public int ItemType { get; set; }

        public ICollection<ProjectItem> Projects { get; set; }
        public ICollection<ObjectiveItem> Objectives { get; set; }
        public ICollection<BimElement> BimElements { get; set; }
    }
}
