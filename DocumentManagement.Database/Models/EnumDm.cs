using System.Collections.Generic;

namespace DocumentManagement.Database.Models
{
    public class EnumDm
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public int ConnectionInfoID { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }

        public ICollection<EnumDmValue> EnumDmValues { get; set; }
    }
}
