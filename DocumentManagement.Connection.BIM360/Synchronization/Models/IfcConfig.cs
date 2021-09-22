using System.Runtime.Serialization;

namespace Brio.Docs.Connection.Bim360.Synchronization.Models
{
    [DataContract]
    internal class IfcConfig
    {
        [DataMember]
        public LinkedInfo RedirectTo { get; set; }
    }
}
