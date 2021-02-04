using System.Runtime.Serialization;

namespace Forge.Models
{
    public class Attachment
    {
        [DataMember(Name = "urn")]
        public string itemId;
        [DataMember(Name = "urn_type")]
        public string type;
        [DataMember(Name = "issue_id")]
        public string issueId;
    }
}
