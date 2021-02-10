using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Models
{
    [DataContract]
    public class Object<TAttributes, TRelationships>
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(Name = "type", EmitDefaultValue = false)]
        public virtual string Type { get; set; }

        [DataMember(Name = "attributes")]
        public TAttributes Attributes { get; set; }

        [DataMember(Name = "relationships", EmitDefaultValue = false)]
        public TRelationships Relationships { get; set; }
    }
}
