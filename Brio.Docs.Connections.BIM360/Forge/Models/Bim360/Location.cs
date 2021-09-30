using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models
{
    [DataContract]
    public class Location
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentID { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "barcode")]
        public string BarCode { get; set; }

        [DataMember(Name = "order")]
        public int Order { get; set; }

        [DataMember(Name = "documentCount")]
        public int DocumentCount { get; set; }

        [DataMember(Name = "areaDefined")]
        public bool AreaDefined { get; set; }
    }
}
