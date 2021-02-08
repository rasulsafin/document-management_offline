using System.Runtime.Serialization;

namespace Forge.Models
{
    [DataContract]
    public class Object<TAttributes, TRelationships>
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string Type { get; set; }

        [DataMember(Name = "attributes")]
        public TAttributes Attributes { get; set; }

        [DataMember(Name = "relationships", EmitDefaultValue = false)]
        public TRelationships Relationships { get; set; }
    }
}
