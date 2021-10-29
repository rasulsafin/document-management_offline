using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360
{
    [DataContract]
    public class Role
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "project_id")]
        public string ProjectID { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "services")]
        public RoleServices Services { get; set; }

        [DataMember(Name = "member_group_id")]
        public string MemberGroupID { get; set; }

        [DataContract]
        public class RoleServices
        {
            [DataMember(Name = "project_administration")]
            public RoleService ProjectAdministration { get; set; }

            [DataMember(Name = "document_management")]
            public RoleService DocumentManagement { get; set; }
        }

        [DataContract]
        public class RoleService
        {
            [DataMember(Name = "access_level")]
            public string AccessLevel { get; set; }
        }
    }
}
