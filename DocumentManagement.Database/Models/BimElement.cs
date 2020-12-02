using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class BimElement
    { 
        public int ID { get; set; }

        public int ItemID { get; set; }
        public Item Item { get; set; }

        public string GlobalID { get; set; }

        public ICollection<BimElementObjective> Objectives { get; set; }
    }
}
