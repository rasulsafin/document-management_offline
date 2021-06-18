using System;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    [DataContract]
    public class LastOnline
    {
        [DataMember(Name = "date")]
        public long Date { get; set; }

        [DataMember(Name = "device")]
        public string Device { get; set; }
    }
}
