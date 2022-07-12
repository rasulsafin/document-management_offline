using System.Collections.Generic;
using System.Runtime.Serialization;
using Brio.Docs.Connections.MrsPro.Interfaces;

namespace Brio.Docs.Connections.MrsPro.Models
{
    [DataContract]
    public class Issue : IElementObject
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "ancestry")]
        public string Ancestry { get; set; }

        [DataMember(Name = "createdDate")]
        public long CreatedDate { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        [DataMember(Name = "parentType")]
        public string ParentType { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [CanBePatched]
        [DataMember(Name = "dueDate")]
        public long DueDate { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [CanBePatched]
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [CanBePatched]
        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "color")]
        public string Color { get; set; }

        [CanBePatched]
        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "lastModifiedDate")]
        public long LastModifiedDate { get; set; }

        [DataMember(Name = "important")]
        public bool Important { get; set; }

        [DataMember(Name = "projectId")]
        public string ProjectId { get; set; }

        [DataMember(Name = "isReopened")]
        public bool IsReopened { get; set; }

        public IEnumerable<IElementAttachment> Attachments { get; set; }

        public bool HasAttachments { get; set; }
    }
}