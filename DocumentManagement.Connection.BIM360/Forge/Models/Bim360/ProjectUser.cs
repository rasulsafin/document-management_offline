using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360
{
    [DataContract]
    public class ProjectUser
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "firstName")]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName")]
        public string LastName { get; set; }

        [DataMember(Name = "autodeskId")]
        public string AutodeskID { get; set; }

        [DataMember(Name = "anaylticsId")]
        public string AnaylticsId { get; set; }

        [DataMember(Name = "addressLine1")]
        public string AddressLine1 { get; set; }

        [DataMember(Name = "addressLine2")]
        public string AddressLine2 { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        [DataMember(Name = "stateOrProvince")]
        public string StateOrProvince { get; set; }

        [DataMember(Name = "postalCode")]
        public int? PostalCode { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "imageUrl")]
        public string ImageUrl { get; set; }

        [DataMember(Name = "phone")]
        public UserPhone Phone { get; set; }

        [DataMember(Name = "jobTitle")]
        public string JobTitle { get; set; }

        [DataMember(Name = "industry")]
        public string Industry { get; set; }

        [DataMember(Name = "aboutMe")]
        public string AboutMe { get; set; }

        [DataMember(Name = "accessLevels")]
        public Accesslevels AccessLevels { get; set; }

        [DataMember(Name = "companyId")]
        public string CompanyId { get; set; }

        [DataMember(Name = "roleIds")]
        public string[] RoleIds { get; set; }

        [DataMember(Name = "services")]
        public Service[] Services { get; set; }

        [DataContract]
        public class UserPhone
        {
            [DataMember(Name = "number")]
            public string Number { get; set; }

            [DataMember(Name = "phoneType")]
            public string PhoneType { get; set; }

            [DataMember(Name = "extension")]
            public string Extension { get; set; }
        }

        [DataContract]
        public class Accesslevels
        {
            [DataMember(Name = "accountAdmin")]
            public bool? AccountAdmin { get; set; }

            [DataMember(Name = "projectAdmin")]
            public bool? ProjectAdmin { get; set; }

            [DataMember(Name = "executive")]
            public bool? Executive { get; set; }
        }

        [DataContract]
        public class Service
        {
            [DataMember(Name = "serviceName")]
            public string ServiceName { get; set; }

            [DataMember(Name = "access")]
            public string Access { get; set; }
        }
    }
}
