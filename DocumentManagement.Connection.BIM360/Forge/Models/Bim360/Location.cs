using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models
{
    [DataContract]
    public class Location : Object<Location.LocationAttributes, object>
    {
        public override string Type
        {
            get => Constants.LOCATION_TYPE;
            set { }
        }

        [DataContract]
        public class LocationAttributes : AAttributes
        {
            [DataMember(Name = "key")]
            public string Key { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }
        }

        [DataContract]
        public class LocationRelationships
        {
            [DataMember(Name = "container")]
            public dynamic Container { get; set; }
        }
    }
}
