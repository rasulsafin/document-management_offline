using System;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class Version : Object<Version.VersionAttributes, Version.VersionRelationships>
    {
        public override string Type
        {
            get => Constants.VERSION_TYPE;
            set { }
        }

        public override string ID { get; set; } = Constants.DEFAULT_VERSION_ID;

        [DataContract]
        public class VersionAttributes : AAttributes
        {
            [DataMember(Name = "createTime")]
            public DateTime? CreateTime { get; set; }

            [DataMember(Name = "createUserId")]
            public string CreateUserId { get; set; }

            [DataMember(Name = "createUserName")]
            public string CreateUserName { get; set; }

            [DataMember(Name = "displayName")]
            public string DisplayName { get; set; }

            [DataMember(Name = "hidden")]
            public bool? Hidden { get; set; }

            [DataMember(Name = "lastModifiedTime")]
            public DateTime? LastModifiedTime { get; set; }

            [DataMember(Name = "lastModifiedUserId")]
            public string LastModifiedUserId { get; set; }

            [DataMember(Name = "lastModifiedUserName")]
            public string LastModifiedUserName { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "reserved")]
            public bool? Reserved { get; set; }

            [DataMember(Name = "reservedTime")]
            public DateTime? ReservedTime { get; set; }

            [DataMember(Name = "reservedUserId")]
            public string ReservedUserId { get; set; }

            [DataMember(Name = "reservedUserName")]
            public string ReservedUserName { get; set; }
        }

        [DataContract]
        public class VersionRelationships
        {
            [DataMember(Name = "links")]
            public dynamic Links { get; set; }

            [DataMember(Name = "derivatives")]
            public dynamic Derivatives { get; set; }

            [DataMember(Name = "downloadFormats")]
            public dynamic DownloadFormats { get; set; }

            [DataMember(Name = "item")]
            public DataContainer<ObjectInfo> Item { get; set; }

            [DataMember(Name = "refs")]
            public dynamic Refs { get; set; }

            [DataMember(Name = "storage")]
            public DataContainer<ObjectInfo> Storage { get; set; }

            [DataMember(Name = "thumbnails")]
            public dynamic Thumbnails { get; set; }
        }
    }
}
