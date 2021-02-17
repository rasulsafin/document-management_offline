using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.LementPro.Models.ObjectBase
{
    [DataContract]
    public class ObjectBaseValue
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public ObjectBaseValueType Type { get; set; }
    }
}
