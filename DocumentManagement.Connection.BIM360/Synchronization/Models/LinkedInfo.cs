using System.Numerics;
using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Synchronization.Models
{
    [DataContract]
    public class LinkedInfo
    {
        [DataMember]
        public string Urn { get; set; }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public Vector3 Offset { get; set; }
    }
}
