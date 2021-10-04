using System;
using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "account_id")]
        public string AccountID { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "role")]
        public string Role { get; set; }

        [DataMember(Name = "company_id")]
        public string CompanyID { get; set; }

        [DataMember(Name = "company_name")]
        public string CompanyName { get; set; }

        [DataMember(Name = "last_sign_in")]
        public DateTime? LastSignIn { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }

        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [DataMember(Name = "uid")]
        public string Uid { get; set; }

        [DataMember(Name = "image_url")]
        public string ImageUrl { get; set; }

        [DataMember(Name = "address_line_1")]
        public string AddressLine1 { get; set; }

        [DataMember(Name = "address_line_2")]
        public string AddressLine2 { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        [DataMember(Name = "postal_code")]
        public string PostalCode { get; set; }

        [DataMember(Name = "state_or_province")]
        public string StateOrProvince { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        [DataMember(Name = "company")]
        public string Company { get; set; }

        [DataMember(Name = "job_title")]
        public string JobTitle { get; set; }

        [DataMember(Name = "industry")]
        public string Industry { get; set; }

        [DataMember(Name = "about_me")]
        public string AboutMe { get; set; }

        [DataMember(Name = "created_at")]
        public DateTime? CreatedAt { get; set; }

        [DataMember(Name = "updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
