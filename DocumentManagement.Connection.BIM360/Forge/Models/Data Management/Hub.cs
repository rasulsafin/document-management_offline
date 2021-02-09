using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    public class Hub : Object<Hub.HubAttributes, Hub.HubRelationships>
    {
        public class HubAttributes : AAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        public class HubRelationships
        {
            [DataMember(Name = "projects")]
            public dynamic Projects { get; set; }
        }
    }
}
