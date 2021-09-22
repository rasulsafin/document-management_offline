using System.Runtime.Serialization;

namespace Brio.Docs.Connections.MrsPro.Models
{
    [DataContract]
    public class UpdatedValues
    {
        [DataMember(Name = "ids")]
        public string[] Ids { get; set; }

        [DataMember(Name = "patch")]
        public Patch[] Patch { get; set; }
    }
}
