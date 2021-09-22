using System.Runtime.Serialization;

namespace Brio.Docs.Connection.LementPro.Models
{
    [DataContract]
    public class BimAttribute
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "attributeId")]
        public string AttributeId { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "showNameInListView")]
        public bool? ShowNameInListView { get; set; }

        [DataMember(Name = "hasChildren")]
        public bool? HasChildren { get; set; }

        [DataMember(Name = "isObject")]
        public bool? IsObject { get; set; }

        [DataMember(Name = "isAttribute")]
        public bool? IsAttribute { get; set; }
    }
}
