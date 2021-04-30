using System;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class Issue : Object<Issue.IssueAttributes, Issue.IssueRelationships>
    {
        public override string Type
        {
            get => Constants.ISSUE_TYPE;
            set { }
        }

        [DataContract]
        public class IssueAttributes
        {
            [DataMember(Name = "created_at")]
            public DateTime? CreatedAt { get; set; }

            [DataMember(Name = "synced_at")]
            public DateTime? SyncedAt { get; set; }

            [DataMember(Name = "updated_at")]
            public DateTime? UpdatedAt { get; set; }

            [DataMember(Name = "close_version")]
            public object CloseVersion { get; set; }

            [DataMember(Name = "closed_at")]
            public object ClosedAt { get; set; }

            [DataMember(Name = "closed_by")]
            public object ClosedBy { get; set; }

            [DataMember(Name = "created_by")]
            public string CreatedBy { get; set; }

            [DataMember(Name = "starting_version")]
            public int? StartingVersion { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }

            [DataMember(Name = "description")]
            public string Description { get; set; }

            [DataMember(Name = "location_description")]
            public string LocationDescription { get; set; }

            [DataMember(Name = "markup_metadata")]
            public object MarkupMetadata { get; set; }

            [DataMember(Name = "tags")]
            public object Tags { get; set; }

            [DataMember(Name = "target_urn")]
            public object TargetUrn { get; set; }

            [DataMember(Name = "snapshot_urn")]
            public object SnapshotUrn { get; set; }

            [DataMember(Name = "target_urn_page")]
            public object TargetUrnPage { get; set; }

            [DataMember(Name = "collection_urn")]
            public object CollectionUrn { get; set; }

            [DataMember(Name = "due_date")]
            public DateTime? DueDate { get; set; }

            [DataMember(Name = "identifier")]
            public int? Identifier { get; set; }

            [DataMember(Name = "status")]
            public Status Status { get; set; } = Status.Draft;

            [DataMember(Name = "assigned_to")]
            public string AssignedTo { get; set; }

            [DataMember(Name = "assigned_to_type")]
            public string AssignedToType { get; set; }

            [DataMember(Name = "answer")]
            public object Answer { get; set; }

            [DataMember(Name = "answered_at")]
            public object AnsweredAt { get; set; }

            [DataMember(Name = "answered_by")]
            public object AnsweredBy { get; set; }

            [DataMember(Name = "pushpin_attributes")]
            public object PushpinAttributes { get; set; }

            [DataMember(Name = "owner")]
            public string Owner { get; set; }

            [DataMember(Name = "issue_type_id")]
            public string IssueTypeID { get; set; }

            [DataMember(Name = "issue_type")]
            public string IssueType { get; set; }

            [DataMember(Name = "issue_sub_type")]
            public object IssueSubType { get; set; }

            [DataMember(Name = "root_cause_id")]
            public string RootCauseID { get; set; }

            [DataMember(Name = "root_cause")]
            public string RootCause { get; set; }

            [DataMember(Name = "quality_urns")]
            public object QualityUrns { get; set; }

            [DataMember(Name = "permitted_statuses")]
            public Status[] PermittedStatuses { get; set; }

            [DataMember(Name = "permitted_attributes")]
            public string[] PermittedAttributes { get; set; }

            [DataMember(Name = "comment_count")]
            public int? CommentCount { get; set; }

            [DataMember(Name = "attachment_count")]
            public int? AttachmentCount { get; set; }

            [DataMember(Name = "permitted_actions")]
            public string[] PermittedActions { get; set; }

            [DataMember(Name = "sheet_metadata")]
            public object SheetMetadata { get; set; }

            [DataMember(Name = "lbs_location")]
            public string LbsLocation { get; set; }

            [DataMember(Name = "ng_issue_subtype_id")]
            public string NgIssueSubtypeID { get; set; }

            [DataMember(Name = "ng_issue_type_id")]
            public string NgIssueTypeID { get; set; }

            [DataMember(Name = "custom_attributes")]
            public object[] CustomAttributes { get; set; }

            [DataMember(Name = "trades")]
            public object Trades { get; set; }

            [DataMember(Name = "comments_attributes")]
            public object CommentsAttributes { get; set; }

            [DataMember(Name = "attachments_attributes")]
            public object AttachmentsAttributes { get; set; }
        }

        [DataContract]
        public class IssueRelationships
        {
            [DataMember(Name = "container")]
            public dynamic Container { get; set; }

            [DataMember(Name = "attachments")]
            public dynamic Attachments { get; set; }

            [DataMember(Name = "activity_batches")]
            public dynamic ActivityBatches { get; set; }

            [DataMember(Name = "comments")]
            public dynamic Comments { get; set; }

            [DataMember(Name = "root_cause_obj")]
            public dynamic RootCauseObj { get; set; }

            [DataMember(Name = "issue_type_obj")]
            public dynamic IssueTypeObj { get; set; }
        }
    }
}
