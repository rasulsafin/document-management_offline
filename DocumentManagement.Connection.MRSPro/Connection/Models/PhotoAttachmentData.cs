using System.Runtime.Serialization;

namespace Brio.Docs.Connection.MrsPro.Models
{
    [DataContract]
    public class PhotoAttachmentData
    {
        [DataMember(Name = "file")]
        public string File { get; set; }
    }
}
