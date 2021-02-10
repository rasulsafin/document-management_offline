using System;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    [DataContract]
    public class Folder : Object<Folder.FolderAttributes, Folder.FolderRelationships>
    {
        public override string Type => Constants.FOLDER_TYPE;

        [DataContract]
        public class FolderAttributes : AAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

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

            [DataMember(Name = "objectCount")]
            public int ObjectCount { get; set; }
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
    }
}