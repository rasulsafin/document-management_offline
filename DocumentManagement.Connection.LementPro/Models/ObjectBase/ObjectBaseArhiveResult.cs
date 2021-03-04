using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    [DataContract]
    public class ObjectBaseArhiveResult
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "state")]
        public int? State { get; set; }
    }
}
