using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class Project : Object<Project.ProjectAttributes, Project.ProjectRelationships>
    {
        [DataContract]
        public class ProjectAttributes : AAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        [DataContract]
        public class ProjectRelationships
        {
            [DataMember(Name = "hub")]
            public dynamic Hub { get; set; }

            [DataMember(Name = "rootFolder")]
            public DataContainer<ObjectInfo> RootFolder { get; set; }

            [DataMember(Name = "topFolders")]
            public dynamic TopFolders { get; set; }

            [DataMember(Name = "issues")]
            public DataContainer<ObjectInfo> IssuesContainer { get; set; }
        }
    }
}
