using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models
{
    [DataContract]
    internal class IfcConfig
    {
        [DataMember]
        public LinkedInfo RedirectTo { get; set; }
    }
}
