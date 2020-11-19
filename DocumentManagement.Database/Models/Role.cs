using System.Collections.Generic;

namespace DocumentManagement.Database.Models
{
    public class Role
    { 
        public int ID { get; set; }
        public string Name { get; set; }

        public ICollection<UserRole> Users { get; set; }
    }
}
