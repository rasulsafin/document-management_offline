using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class Hub : Object<Hub.HubAttributes, Hub.HubRelationships>
    {
        [DataContract]
        public class HubAttributes : AAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        [DataContract]
        public class HubRelationships
        {
            [DataMember(Name = "projects")]
            public dynamic Projects { get; set; }
        }
    }
}
