using System;
using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class Folder : Object<Folder.FolderAttributes, Folder.FolderRelationships>
    {
        public override string Type
        {
            get => Constants.FOLDER_TYPE;
            set { }
        }

        [DataContract]
        public class FolderAttributes : AAttributes<FolderExtension>
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "createTime")]
            public DateTime? CreateTime { get; set; }

            [DataMember(Name = "createUserId")]
            public string CreateUserId { get; set; }

            [DataMember(Name = "createUserName")]
            public string CreateUserName { get; set; }

            [DataMember(Name = "displayName")]
            public string DisplayName { get; set; }

            [DataMember(Name = "hidden")]
            public bool Hidden { get; set; }

            [DataMember(Name = "lastModifiedTime")]
            public DateTime? LastModifiedTime { get; set; }

            [DataMember(Name = "lastModifiedUserId")]
            public string LastModifiedUserId { get; set; }

            [DataMember(Name = "lastModifiedUserName")]
            public string LastModifiedUserName { get; set; }

            [DataMember(Name = "objectCount")]
            public int? ObjectCount { get; set; }
        }

        [DataContract]
        public class FolderRelationships
        {
            [DataMember(Name = "links")]
            public dynamic Links { get; set; }

            [DataMember(Name = "contents")]
            public dynamic Contents { get; set; }

            [DataMember(Name = "parent")]
            public dynamic Parent { get; set; }

            [DataMember(Name = "refs")]
            public dynamic Refs { get; set; }
        }

        [DataContract]
        public class FolderExtension : Extension
        {
            [DataMember(Name = "data")]
            public FolderExtensionData Data { get; set; }
        }

        [DataContract]
        public class FolderExtensionData
        {
            [DataMember(Name = "visibleTypes")]
            public string[] VisibleTypes { get; set; }

            [DataMember(Name = "actions")]
            public string[] Actions { get; set; }

            [DataMember(Name = "allowedTypes")]
            public string[] AllowedTypes { get; set; }
        }
    }
}