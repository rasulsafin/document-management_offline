using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Models
{
    [DataContract]
    public class Extension
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}