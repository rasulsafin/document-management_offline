using System.Runtime.Serialization;

namespace Forge.Models.DataManagement
{
    public abstract class AAttributes
    {
        [DataMember(Name = "extension")]
        public Extension Extension { get; set; }
    }
}
