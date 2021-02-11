using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class Extension
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}