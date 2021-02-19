using System;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class IssueSubtype
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "issueTypeId")]
        public string IssueTypeId { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "createdAt")]
        public DateTime? CreatedAt { get; set; }

        [DataMember(Name = "updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [DataMember(Name = "deletedAt")]
        public object DeletedAt { get; set; }

        [DataMember(Name = "isActive")]
        public bool IsActive { get; set; }

        [DataMember(Name = "orderIndex")]
        public int? OrderIndex { get; set; }

        [DataMember(Name = "isReadOnly")]
        public bool IsReadOnly { get; set; }

        [DataMember(Name = "permittedActions")]
        public string[] PermittedActions { get; set; }

        [DataMember(Name = "permittedAttributes")]
        public string[] PermittedAttributes { get; set; }
    }
}
