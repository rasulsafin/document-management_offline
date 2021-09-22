using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models
{
    [DataContract]
    public class RequestData
    {
        [DataMember(Name = "jsonapi")]
        public JsonApi? JsonApi { get; set; }

        [DataMember(Name = "data")]
        public object Data { get; set; }

        [DataMember(Name = "included")]
        public object Included { get; set; }
    }
}
