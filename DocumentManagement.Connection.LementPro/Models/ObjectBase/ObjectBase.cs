using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class ObjectBase
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "uniqueId")]
        public string UniqueId { get; set; }

        [DataMember(Name = "objectUrl")]
        public string ObjectUrl { get; set; }

        [DataMember(Name = "isObject")]
        public bool? IsObject { get; set; }

        [DataMember(Name = "values")]
        public ObjectBaseValue Values { get; set; }

        [DataMember(Name = "isClosed")]
        public bool? IsClosed { get; set; }

        [DataMember(Name = "isUnread")]
        public bool? IsUnread { get; set; }

        [DataMember(Name = "isMarked")]
        public bool? IsMarked { get; set; }

        [DataMember(Name = "CanRead")]
        public bool? CanRead { get; set; }

        [DataMember(Name = "CanEdit")]
        public bool? CanEdit { get; set; }

        [DataMember(Name = "CanDelete")]
        public bool? CanDelete { get; set; }

        [DataMember(Name = "CanUnread")]
        public bool? CanUnread { get; set; }

        [DataMember(Name = "state")]
        public int? State { get; set; }

        [DataMember(Name = "isExpired")]
        public bool? IsExpired { get; set; }

        [DataMember(Name = "isEditable")]
        public bool? IsEditable { get; set; }

        [DataMember(Name = "isRouteTask")]
        public bool? IsRouteTask { get; set; }

        [DataMember(Name = "hasChildren")]
        public bool? HasChildren { get; set; }

        [DataMember(Name = "newCommentCount")]
        public int? NewCommentCount { get; set; }

        [DataMember(Name = "lastComment")]
        public dynamic LastComment { get; set; }

        [DataMember(Name = "hasExpiredCheckpoints")]
        public bool? HasExpiredCheckpoints { get; set; }
    }
}
