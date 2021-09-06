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
            public bool Is_project_admin { get; set; }

            [DataMember(Name = "workflow_roles")]
            public string[] Workflow_roles { get; set; }

            [DataMember(Name = "workflow_type")]
            public string Workflow_type { get; set; }

            [DataMember(Name = "rfis")]
            public Rfis Rfis { get; set; }

            [DataMember(Name = "quality_issues")]
            public Quality_Issues Quality_issues { get; set; }
        }

        [DataContract]
        public class Rfis
        {
            [DataMember(Name = "workflow_roles")]
            public string[] Workflow_roles { get; set; }

            [DataMember(Name = "workflow_type")]
            public string Workflow_type { get; set; }

            [DataMember(Name = "_new")]
            public New _new { get; set; }
        }

        [DataContract]
        public class New
        {
            [DataMember(Name = "required_attributes")]
            public string[] Required_attributes { get; set; }

            [DataMember(Name = "optional_attributes")]
            public string[] Optional_attributes { get; set; }

            [DataMember(Name = "initial_data")]
            public Initial_Data[] Initial_data { get; set; }
        }

        [DataContract]
        public class Initial_Data
        {
            [DataMember(Name = "status")]
            public string Status { get; set; }

            [DataMember(Name = "workflow_state")]
            public Workflow_State Workflow_state { get; set; }

            [DataMember(Name = "assignees")]
            public Assignee[] Assignees { get; set; }

            [DataMember(Name = "transitions")]
            public Transition[] Transitions { get; set; }
        }

        [DataContract]
        public class Workflow_State
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }

            [DataMember(Name = "short_title")]
            public string Short_title { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "status")]
            public string Status { get; set; }
        }

        [DataContract]
        public class Assignee
        {
            [DataMember(Name = "type")]
            public string Type { get; set; }

            [DataMember(Name = "id")]
            public string Id { get; set; }
        }

        [DataContract]
        public class Transition
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }

            [DataMember(Name = "status")]
            public string Status { get; set; }

            [DataMember(Name = "workflow_state")]
            public Workflow_State1 Workflow_state { get; set; }

            [DataMember(Name = "required_attributes")]
            public string[] Required_attributes { get; set; }
        }

        [DataContract]
        public class Workflow_State1
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }

            [DataMember(Name = "short_title")]
            public string Short_title { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }

        [DataContract]
        public class Quality_Issues
        {
            [DataMember(Name = "_new")]
            public New1 _new { get; set; }
        }

        [DataContract]
        public class New1
        {
            [DataMember(Name = "permitted_actions")]
            public string[] Permitted_actions { get; set; }

            [DataMember(Name = "permitted_statuses")]
            public string[] Permitted_statuses { get; set; }

            [DataMember(Name = "permitted_attributes")]
            public string[] Permitted_attributes { get; set; }
        }
    }
}
