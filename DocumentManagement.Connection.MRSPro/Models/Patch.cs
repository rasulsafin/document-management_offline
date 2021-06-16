using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    [DataContract]
    public class Patch
    {
        [DataMember(Name = "op")]
        public string Operation { get; set; } = Constants.OP_REPLACE;

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "value")]
        public dynamic Value { get; set; }
    }
}
