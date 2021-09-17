using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360
{
    [DataContract]
    public class UserInfo
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "attributes")]
        public UserAttributes Attributes { get; set; }

        [DataContract]
        public class UserAttributes
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "role")]
            public string Role { get; set; }

            [DataMember(Name = "is_project_admin")]
            public bool IsProjectAdmin { get; set; }

            [DataMember(Name = "workflow_roles")]
            public string[] WorkflowRoles { get; set; }

            [DataMember(Name = "workflow_type")]
            public string WorkflowType { get; set; }

            [DataMember(Name = "rfis")]
            public object Rfis { get; set; }

            [DataMember(Name = "quality_issues")]
            public QualityIssues QualityIssues { get; set; }
        }

        [DataContract]
        public class QualityIssues
        {
            [DataMember(Name = "new")]
            public InitialInfo New { get; set; }
        }

        [DataContract]
        public class InitialInfo
        {
            [DataMember(Name = "permitted_actions")]
            public string[] PermittedActions { get; set; }

            [DataMember(Name = "permitted_statuses")]
            public string[] PermittedStatuses { get; set; }

            [DataMember(Name = "permitted_attributes")]
            public string[] PermittedAttributes { get; set; }
        }
    }
}
