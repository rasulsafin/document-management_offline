using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Synchronization.Models
{
    [DataContract]
    internal class IfcConfig
    {
        [DataMember]
        public LinkedInfo RedirectTo { get; set; }
    }
}
