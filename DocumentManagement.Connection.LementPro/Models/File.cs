using System.Runtime.Serialization;

namespace Brio.Docs.Connection.LementPro.Models
{
    [DataContract]
    public class File
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "fileId")]
        public string FileId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "revision")]
        public int? Revision { get; set; }

        [DataMember(Name = "size")]
        public string Size { get; set; }

        [DataMember(Name = "author")]
        public dynamic Author { get; set; }

        [DataMember(Name = "authorFrom")]
        public dynamic AuthorFrom { get; set; }

        [DataMember(Name = "creationDate")]
        public string CreationDate { get; set; }

        [DataMember(Name = "isFinal")]
        public bool? IsFinal { get; set; }

        [DataMember(Name = "isInStorage")]
        public bool? IsInStorage { get; set; }
    }
}
