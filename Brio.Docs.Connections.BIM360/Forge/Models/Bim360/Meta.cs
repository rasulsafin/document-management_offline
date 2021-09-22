using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
{
    [DataContract(Name = "meta")]
    public class Meta
    {
        [DataMember(Name = "page")]
        public MetaPage Page { get; set; }

        [DataMember(Name = "record_count")]
        public int? RecordCount { get; set; }

        [DataContract]
        public class MetaPage
        {
            [DataMember(Name = "offset")]
            public int? Offset { get; set; }

            [DataMember(Name = "limit")]
            public int? Limit { get; set; }
        }
    }
}
