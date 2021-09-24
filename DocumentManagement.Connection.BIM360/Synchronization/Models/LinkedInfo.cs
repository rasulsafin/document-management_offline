using System.Numerics;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models
{
    [DataContract]
    internal class LinkedInfo
    {
        [DataMember]
        public string Urn { get; set; }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public Vector3 Offset { get; set; }
    }
}
