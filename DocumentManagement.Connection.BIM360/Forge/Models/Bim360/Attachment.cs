using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class Attachment : Object<Attachment.AttachmentAttributes, Attachment.AttachmentRelationships>
    {
        [DataContract]
        public class AttachmentAttributes
        {
            [DataMember(Name = "urn")]
            public string Urn { get; set; }

            [DataMember(Name = "urn_type")]
            public string UrnType { get; set; }

            [DataMember(Name = "issue_id")]
            public string IssueId { get; set; }
        }

        [DataContract]
        public class AttachmentRelationships
        {
            [DataMember(Name = "activity_batches")]
            public dynamic ActivityBatches { get; set; }

            [DataMember(Name = "issue")]
            public dynamic Issue { get; set; }
        }
    }
}
