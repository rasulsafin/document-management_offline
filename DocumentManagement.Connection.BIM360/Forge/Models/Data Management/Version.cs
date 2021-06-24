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
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "displayName")]
            public string DisplayName { get; set; }

            [DataMember(Name = "createTime")]
            public DateTime? CreateTime { get; set; }

            [DataMember(Name = "createUserId")]
            public string CreateUserID { get; set; }

            [DataMember(Name = "createUserName")]
            public string CreateUserName { get; set; }

            [DataMember(Name = "lastModifiedTime")]
            public DateTime? LastModifiedTime { get; set; }

            [DataMember(Name = "lastModifiedUserId")]
            public string LastModifiedUserID { get; set; }

            [DataMember(Name = "lastModifiedUserName")]
            public string LastModifiedUserName { get; set; }

            [DataMember(Name = "versionNumber")]
            public int? VersionNumber { get; set; }

            [DataMember(Name = "storageSize")]
            public int? StorageSize { get; set; }

            [DataMember(Name = "fileType")]
            public string FileType { get; set; }
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
