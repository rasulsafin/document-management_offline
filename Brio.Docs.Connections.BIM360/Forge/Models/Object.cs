using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models
{
    [DataContract]
    public class Object<TAttributes, TRelationships>
    {
        [DataMember(Name = "id")]
        public virtual string ID { get; set; }

        [DataMember(Name = "type")]
        public virtual string Type { get; set; }

        [DataMember(Name = "attributes")]
        public TAttributes Attributes { get; set; }

        [DataMember(Name = "relationships")]
        public TRelationships Relationships { get; set; }
    }
}
