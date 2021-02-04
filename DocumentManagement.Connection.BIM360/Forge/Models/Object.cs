using System.Runtime.Serialization;

namespace Forge.Models
{
    [DataContract]
    public class Object<TAttributes, TRelationships>
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "attributes")]
        public TAttributes Attributes { get; set; }

        [DataMember(Name = "relationships")]
        public TRelationships Relationships { get; set; }
    }
}
