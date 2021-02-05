using System.Runtime.Serialization;

namespace Forge.Models
{
    public class Meta
    {
        [DataMember(Name = "page")]
        public Page Page { get; set; }

        [DataMember(Name = "record_count")]
        public int RecordCount { get; set; }
    }
}