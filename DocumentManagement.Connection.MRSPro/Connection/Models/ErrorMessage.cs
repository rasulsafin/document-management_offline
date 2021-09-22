using System.Runtime.Serialization;

namespace Brio.Docs.Connection.MrsPro.Models
{
    [DataContract]
    public class ErrorMessage
    {
        [DataMember(Name = "timestamp")]
        public long Timestamp { get; set; }

        [DataMember(Name = "status")]
        public int Status { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }
    }
}
