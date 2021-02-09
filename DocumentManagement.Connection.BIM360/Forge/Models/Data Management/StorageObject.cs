using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    [DataContract]
    public class StorageObject : Object<StorageObject.StorageObjectAttributes, StorageObject.StorageObjectRelationships>
    {
        [DataContract]
        public class StorageObjectAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        [DataContract]
        public class StorageObjectRelationships
        {
            [DataMember(Name = "target")]
            public dynamic Target { get; set; }
        }
    }
}
