using System;
using System.Runtime.Serialization;

namespace Forge.Models.DataManagement
{
    public class Version : Object<Version.VersionAttributes, Version.VersionRelationships>
    {
        public class VersionAttributes : AAttributes
        {
            [DataMember(Name = "createTime")]
            public DateTime CreateTime { get; set; }

            [DataMember(Name = "createUserId")]
            public string CreateUserId { get; set; }

            [DataMember(Name = "createUserName")]
            public string CreateUserName { get; set; }

            [DataMember(Name = "displayName")]
            public string DisplayName { get; set; }

            [DataMember(Name = "hidden")]
            public bool Hidden { get; set; }

            [DataMember(Name = "lastModifiedTime")]
            public DateTime LastModifiedTime { get; set; }

            [DataMember(Name = "lastModifiedUserId")]
            public string LastModifiedUserId { get; set; }

            [DataMember(Name = "lastModifiedUserName")]
            public string LastModifiedUserName { get; set; }

            [DataMember(Name = "reserved")]
            public bool Reserved { get; set; }

            [DataMember(Name = "reservedTime")]
            public DateTime ReservedTime { get; set; }

            [DataMember(Name = "reservedUserId")]
            public string ReservedUserId { get; set; }

            [DataMember(Name = "reservedUserName")]
            public string ReservedUserName { get; set; }
        }

        public class VersionRelationships
        {
            [DataMember(Name = "links")]
            public dynamic Links { get; set; }

            [DataMember(Name = "derivatives")]
            public dynamic Derivatives { get; set; }

            [DataMember(Name = "downloadFormats")]
            public dynamic DownloadFormats { get; set; }

            [DataMember(Name = "item")]
            public dynamic Item { get; set; }

            [DataMember(Name = "refs")]
            public dynamic Refs { get; set; }

            [DataMember(Name = "storage")]
            public dynamic Storage { get; set; }

            [DataMember(Name = "thumbnails")]
            public dynamic Thumbnails { get; set; }
        }
    }
}