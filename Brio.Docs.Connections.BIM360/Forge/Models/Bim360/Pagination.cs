using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
{
    [DataContract(Name = "pagination")]
    public class Pagination
    {
        [DataMember(Name = "limit")]
        public int? Limit { get; set; }

        [DataMember(Name = "offset")]
        public int? Offset { get; set; }

        [DataMember(Name = "totalResults")]
        public int? TotalResults { get; set; }
    }
}
