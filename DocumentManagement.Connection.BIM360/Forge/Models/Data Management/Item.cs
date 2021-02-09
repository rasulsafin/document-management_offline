using System;
using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    [DataContract]
    public class Item : Object<Item.ItemAttributes, Item.ItemRelationships>
    {
        [DataContract]
        public class ItemAttributes : AAttributes
        {
            [DataMember(Name = "createTime")]
            public DateTime CreateTime { get; set; }

            [DataMember(Name = "createUserId")]
            public string CreateUserId { get; set; }

            [DataMember(Name = "createUserName")]
            public string CreateUserName { get; set; }

            [DataMember(Name = "displayName")]
            public string DisplayName { get; set; }

            [DataMember(Name = "lastModifiedTime")]
            public DateTime LastModifiedTime { get; set; }

            [DataMember(Name = "lastModifiedUserId")]
            public string LastModifiedUserId { get; set; }

            [DataMember(Name = "lastModifiedUserName")]
            public string LastModifiedUserName { get; set; }

            [DataMember(Name = "mimeType")]
            public string MimeType { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "versionNumber")]
            public int VersionNumber { get; set; }
        }

        [DataContract]
        public class ItemRelationships
        {
            [DataMember(Name = "links")]
            public dynamic Links { get; set; }

            [DataMember(Name = "parent")]
            public dynamic Parent { get; set; }

            [DataMember(Name = "refs")]
            public dynamic Refs { get; set; }

            [DataMember(Name = "tip")]
            public dynamic Tip { get; set; }

            [DataMember(Name = "versions")]
            public dynamic Versions { get; set; }
        }
    }
}
