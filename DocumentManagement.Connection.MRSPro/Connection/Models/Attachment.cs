using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.MrsPro.Interfaces;

namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    [DataContract]
    public class Attachment : IElementAttachment
    {
        [DataMember(Name = "address")]
        public string Address { get; set; }

        [DataMember(Name = "ancestry")]
        public string Ancestry { get; set; }

        [DataMember(Name = "archiveInitiator")]
        public string ArchiveInitiator { get; set; }

        [DataMember(Name = "archivedDate")]
        public long ArchivedDate { get; set; }

        [DataMember(Name = "clonedFromId")]
        public string ClonedFromId { get; set; }

        [DataMember(Name = "contentType")]
        public string ContentType { get; set; }

        [DataMember(Name = "createdDate")]
        public long CreatedDate { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "fileSize")]
        public int FileSize { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "latitude")]
        public float Latitude { get; set; }

        [DataMember(Name = "longitude")]
        public float Longitude { get; set; }

        [DataMember(Name = "originalName")]
        public string OriginalName { get; set; }

        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "parentType")]
        public string ParentType { get; set; }

        [DataMember(Name = "restoredDate")]
        public long RestoredDate { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "urlToFile")]
        public string UrlToFile { get; set; }

        [DataMember(Name = "urlToThumbnail")]
        public string UrlToThumbnail { get; set; }

        [DataMember(Name = "orinalFileName")]
        public string OriginalFileName { get; set; }
    }
}
