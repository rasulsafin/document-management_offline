using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class Page
    {
        [DataMember(Name = "offset")]
        public int Offset { get; set; }

        [DataMember(Name = "limit")]
        public int Limit { get; set; }
    }

}