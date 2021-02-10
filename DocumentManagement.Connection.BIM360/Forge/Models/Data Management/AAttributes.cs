using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    [DataContract]
    public abstract class AAttributes
    {
        [DataMember(Name = "extension")]
        public Extension Extension { get; set; }
    }
}
