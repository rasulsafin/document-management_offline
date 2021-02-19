using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class ObjectiveType
    { 
        public int ID { get; set; }
        public string Name { get; set; }

        public ICollection<Objective> Objectives { get; set; }
    }
}
