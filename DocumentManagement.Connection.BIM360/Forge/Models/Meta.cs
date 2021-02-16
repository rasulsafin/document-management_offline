using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class Meta
    {
        [DataMember(Name = "page")]
        public Page Page { get; set; }

        [DataMember(Name = "record_count")]
        public int? RecordCount { get; set; }
    }
}