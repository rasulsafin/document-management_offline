using System;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class Item : Object<Item.ItemAttributes, Item.ItemRelationships>
    {
        public override string Type
        {
            get => Constants.ITEM_TYPE;
            set { }
        }

        [DataContract]
        public class ItemAttributes : AAttributes
        {
            [DataMember(Name = "displayName")]
            public string DisplayName { get; set; }

            [DataMember(Name = "createTime")]
            public DateTime? CreateTime { get; set; }

            [DataMember(Name = "createUserId")]
            public string CreateUserId { get; set; }

            [DataMember(Name = "createUserName")]
            public string CreateUserName { get; set; }

            [DataMember(Name = "lastModifiedTime")]
            public DateTime? LastModifiedTime { get; set; }

            [DataMember(Name = "lastModifiedUserId")]
            public string LastModifiedUserId { get; set; }

            [DataMember(Name = "lastModifiedUserName")]
            public string LastModifiedUserName { get; set; }

            [DataMember(Name = "hidden")]
            public bool? Hidden { get; set; }

            [DataMember(Name = "reserved")]
            public bool? Reserved { get; set; }
        }

        [DataContract]
        public class ItemRelationships
        {
            [DataMember(Name = "links")]
            public dynamic Links { get; set; }

            [DataMember(Name = "parent")]
            public DataContainer<ObjectInfo> Parent { get; set; }

            [DataMember(Name = "refs")]
            public dynamic Refs { get; set; }

            [DataMember(Name = "tip")]
            public DataContainer<ObjectInfo> Tip { get; set; }

            [DataMember(Name = "versions")]
            public dynamic Versions { get; set; }
        }
    }
}
