using System;
using System.Runtime.Serialization;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
{
    [DataContract]
    public class Attachment : Object<Attachment.AttachmentAttributes, Attachment.AttachmentRelationships>
    {
        public override string Type
        {
            get => Constants.ATTACHMENT_TYPE;
            set { }
        }

        [DataContract]
        public class AttachmentAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "urn")]
            public string Urn { get; set; }

            [DataMember(Name = "urn_type")]
            public UrnType? UrnType { get; set; }

            [DataMember(Name = "issue_id")]
            public string IssueId { get; set; }

            [DataMember(Name = "created_at")]
            public DateTime? CreatedAt { get; set; }

            [DataMember(Name = "synced_at")]
            public DateTime? SyncedAt { get; set; }

            [DataMember(Name = "updated_at")]
            public DateTime? UpdatedAt { get; set; }

            [DataMember(Name = "attachment_type")]
            public string AttachmentType { get; set; }

            [DataMember(Name = "created_by")]
            public string CreatedBy { get; set; }

            [DataMember(Name = "markup_metadata")]
            public object MarkupMetadata { get; set; }

            [DataMember(Name = "url")]
            public string Url { get; set; }

            [DataMember(Name = "urn_page")]
            public int? UrnPage { get; set; }

            [DataMember(Name = "urn_version")]
            public int? UrnVersion { get; set; }

            [DataMember(Name = "permitted_actions")]
            public string[] PermittedActions { get; set; }
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
