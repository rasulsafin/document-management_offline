using System.Linq;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    [DataContract]
    public class StorageObject : Object<StorageObject.StorageObjectAttributes, StorageObject.StorageObjectRelationships>
    {
        public override string Type
        {
            get => Constants.OBJECT_TYPE;
            set { }
        }

        public (string bucketKey, string hashedName) ParseStorageId()
        {
            var bucketKey = ID.Split(':').Last().Split('/').First();
            var hashedName = ID.Split('/').Last();

            return (bucketKey, hashedName);
        }

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
