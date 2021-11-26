using System.Runtime.Serialization;

namespace Brio.Docs.Connections.LementPro.Models
{
    [DataContract]
    public class FolderKey
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "subKeys")]
        public dynamic SubKeys { get; set; }
    }
}
