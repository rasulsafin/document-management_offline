using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    public class StorageObject : Object<StorageObject.StorageObjectAttributes, StorageObject.StorageObjectRelationshops>
    {
        public class StorageObjectAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        public class StorageObjectRelationshops
        {
            [DataMember(Name = "target")]
            public dynamic Target { get; set; }
        }
    }
}
