using System.Runtime.Serialization;

namespace Forge.Models
{
    public class Page
    {
        [DataMember(Name = "offset")]
        public int Offset { get; set; }

        [DataMember(Name = "limit")]
        public int Limit { get; set; }
    }

}