using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    public class Project : Object<Project.ProjectAttributes, Project.ProjectRelationships>
    {
        public class ProjectAttributes : AAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        public class ProjectRelationships
        {
            [DataMember(Name = "hub")]
            public dynamic Hub { get; set; }

            [DataMember(Name = "rootFolder")]
            public dynamic RootFolder { get; set; }

            [DataMember(Name = "topFolders")]
            public dynamic TopFolders { get; set; }
        }
    }
}