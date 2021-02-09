using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models
{
    public class Extension
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}