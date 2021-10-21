using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models
{
    [DataContract]
    public class ObjectInfo
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}
