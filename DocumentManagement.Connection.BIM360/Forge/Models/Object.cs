using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class Object<TAttributes, TRelationships>
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "type")]
        public virtual string Type { get; set; }

        [DataMember(Name = "attributes")]
        public TAttributes Attributes { get; set; }

        [DataMember(Name = "relationships")]
        public TRelationships Relationships { get; set; }
    }
}
