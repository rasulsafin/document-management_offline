using System;
using System.Numerics;
using System.Runtime.Serialization;
using Brio.Docs.Connections.Bim360.Forge.Utils.JsonConverters;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
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
            public string TargetUrn { get; set; }

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
            [JsonProperty(NullValueHandling = NullValueHandling.Include)]
            public string AssignedTo { get; set; }

            [DataMember(Name = "assigned_to_type")]
            public AssignToType? AssignedToType { get; set; }

            [DataMember(Name = "answer")]
            public string Answer { get; set; }

            [DataMember(Name = "answered_at")]
            public object AnsweredAt { get; set; }

            [DataMember(Name = "answered_by")]
            public object AnsweredBy { get; set; }

            [DataMember(Name = "pushpin_attributes")]
            public PushpinAttributes PushpinAttributes { get; set; }

            [DataMember(Name = "owner")]
            public string Owner { get; set; }

            [DataMember(Name = "issue_type_id")]
            public string IssueTypeID { get; set; }

            [DataMember(Name = "issue_type")]
            public string IssueType { get; set; }

            [DataMember(Name = "issue_sub_type")]
            public object IssueSubType { get; set; }

            [DataMember(Name = "root_cause_id")]
            [JsonProperty(NullValueHandling = NullValueHandling.Include)]
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
            public SheetMetadata SheetMetadata { get; set; }

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

        [DataContract]
        public class PushpinAttributes
        {
            [DataMember(Name = "type")]
            public object Type { get; set; } = Constants.PUSHPIN_TYPE;

            [DataMember(Name = "location")]
            [JsonConverter(typeof(Vector3Vector3LowercaseConverter))]
            public Vector3? Location { get; set; }

            [DataMember(Name = "object_id")]
            public string ObjectID { get; set; }

            [DataMember(Name = "viewer_state")]
            public ViewerState ViewerState { get; set; }

            [DataMember(Name = "created_at")]
            public object CreatedAt { get; set; }

            [DataMember(Name = "created_by")]
            public object CreatedBy { get; set; }

            [DataMember(Name = "created_doc_version")]
            public object CreatedDocVersion { get; set; }

            [DataMember(Name = "external_id")]
            public object ExternalID { get; set; }

            [DataMember(Name = "attributes_version")]
            public object AttributesVersion { get; set; }
        }

        [DataContract]
        public class ViewerState
        {
            [DataMember(Name = "seedURN")]
            public string SeedUrn { get; set; }

            [DataMember(Name = "viewport")]
            public Viewport Viewport { get; set; }

            [DataMember(Name = "cutplanes")]
            public object CutPlanes { get; set; }

            [DataMember(Name = "floorGuid")]
            public object FloorGuid { get; set; }

            [DataMember(Name = "objectSet")]
            public object ObjectSet { get; set; }

            [DataMember(Name = "globalOffset")]
            [JsonConverter(typeof(Vector3Vector3LowercaseConverter))]
            public Vector3? GlobalOffset { get; set; }

            [DataMember(Name = "renderOptions")]
            public object RenderOptions { get; set; }

            [DataMember(Name = "attributesVersion")]
            public object AttributesVersion { get; set; }

            /// <summary>
            /// Our property to save our additional info.
            /// </summary>
            [DataMember]
            public object OtherInfo { get; set; }
        }

        [DataContract]
        public class SheetMetadata
        {
            [DataMember(Name = "is3D")]
            public bool Is3D { get; set; } = true;

            [DataMember(Name = "sheetGuid")]
            public string Guid { get; set; }

            [DataMember(Name = "sheetName")]
            public string Name { get; set; }
        }

        [DataContract]
        public class Viewport
        {
            [DataMember(Name = "up")]
            [JsonConverter(typeof(NullableVector3FloatArrayConverter))]
            public Vector3? Up { get; set; }

            [DataMember(Name = "eye")]
            [JsonConverter(typeof(NullableVector3StringArrayConverter))]
            public Vector3? Eye { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "target")]
            [JsonConverter(typeof(NullableVector3StringArrayConverter))]
            public Vector3? Target { get; set; }

            [DataMember(Name = "pivotPoint")]
            [JsonConverter(typeof(NullableVector3StringArrayConverter))]
            public Vector3? PivotPoint { get; set; }

            [DataMember(Name = "projection")]
            public string Projection { get; set; }

            [DataMember(Name = "aspectRatio")]
            public float? AspectRatio { get; set; }

            [DataMember(Name = "fieldOfView")]
            public float? FieldOfView { get; set; }

            [DataMember(Name = "worldUpVector")]
            [JsonConverter(typeof(NullableVector3FloatArrayConverter))]
            public Vector3? WorldUpVector { get; set; }

            [DataMember(Name = "isOrthographic")]
            public bool? IsOrthographic { get; set; }

            [DataMember(Name = "distanceToOrbit")]
            public float? DistanceToOrbit { get; set; }
        }
    }
}
