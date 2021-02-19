using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class EnumDmValue
    {
        public int ID { get; set; }
        public string Value { get; set; }

        public int EnumDmID { get; set; }
        public EnumDm EnumDm { get; set; }

        public ICollection<UserEnumDmValue> Users { get; set; }
    }
}
