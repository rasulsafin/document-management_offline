using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    [DataContract]
    public class PhotoAttachmentData
    {
        [DataMember(Name = "file")]
        public string File { get; set; }
    }
}
