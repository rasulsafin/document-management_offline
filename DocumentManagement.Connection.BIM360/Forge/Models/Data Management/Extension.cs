using System.Runtime.Serialization;

namespace Forge.Models
{
    public class Extension
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}