using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "accessToken")]
        public string AccessToken { get; set; }

        [DataMember(Name = "ancestry")]
        public string Ancestry { get; set; }

        [DataMember(Name = "archiveInitiator")]
        public string ArchiveInitiatorestry { get; set; }

        [DataMember(Name = "archivedDate")]
        public string ArchivedDate { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "folder")]
        public bool Folder { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "isLicensed")]
        public bool IsLicensed { get; set; }

        [DataMember(Name = "isLocked")]
        public bool IsLocked { get; set; }

        [DataMember(Name = "lang")]
        public string Lang { get; set; }

        [DataMember(Name = "last_online")]
        public LastOnline LastOnline { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "nameForReport")]
        public string NameForReport { get; set; }

        [DataMember(Name = "organizationId")]
        public string OrganizationId { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "parentType")]
        public object ParentType { get; set; }

        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        [DataMember(Name = "position")]
        public string Position { get; set; }

        [DataMember(Name = "positionForReport")]
        public string PositionForReport { get; set; }

        [DataMember(Name = "registerDate")]
        public string RegisterDate { get; set; }

        [DataMember(Name = "restoredDate")]
        public string RestoredDate { get; set; }

        [DataMember(Name = "settings")]
        public Settings Settings { get; set; }

        [DataMember(Name = "type")]
        public object Type { get; set; }

        [DataMember(Name = "userId")]
        public string UserId { get; set; }
    }
}
