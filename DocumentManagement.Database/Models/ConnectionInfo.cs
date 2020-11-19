using System.Collections.Generic;

namespace DocumentManagement.Database.Models
{
    public class ConnectionInfo
    { 
        public int ID { get; set; }
        public string Name { get; set; }
        public string AuthFieldNames { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<EnumDm> EnumDms { get; set; }
    }
}
