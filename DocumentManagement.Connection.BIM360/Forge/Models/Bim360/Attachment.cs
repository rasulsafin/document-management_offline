using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models
{
    public class Attachment : Object<Attachment.AttachmentAttributes, Attachment.AttachmentRelationships>
    {
        public class AttachmentAttributes
        {
            [DataMember(Name = "urn")]
            public string Urn { get; set; }

            [DataMember(Name = "urn_type")]
            public string UrnType { get; set; }

            [DataMember(Name = "issue_id")]
            public string IssueId { get; set; }
        }

        public class AttachmentRelationships
        {
            [DataMember(Name = "activity_batches")]
            public dynamic ActivityBatches { get; set; }

            [DataMember(Name = "issue")]
            public dynamic Issue { get; set; }
        }
    }
}
