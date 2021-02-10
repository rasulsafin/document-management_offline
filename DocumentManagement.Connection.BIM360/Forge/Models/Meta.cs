using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Models
{
    [DataContract]
    public class Meta
    {
        [DataMember(Name = "page")]
        public Page Page { get; set; }

        [DataMember(Name = "record_count")]
        public int RecordCount { get; set; }
    }
}