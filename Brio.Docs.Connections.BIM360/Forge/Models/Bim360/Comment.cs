using System;
using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
{
    [DataContract]
    public class Comment : Object<Comment.CommentAttributes, object>
    {
        public override string Type
        {
            get => Constants.COMMENT_TYPE; set { }
        }

        [DataContract]
        public class CommentAttributes
        {
            [DataMember(Name = "created_at")]
            public DateTime? CreatedAt { get; set; }

            [DataMember(Name = "synced_at")]
            public DateTime? SyncedAt { get; set; }

            [DataMember(Name = "updated_at")]
            public DateTime? UpdatedAt { get; set; }

            [DataMember(Name = "issue_id")]
            public string IssueId { get; set; }

            [DataMember(Name = "body")]
            public string Body { get; set; }

            [DataMember(Name = "created_by")]
            public string CreatedBy { get; set; }
        }
    }
}
