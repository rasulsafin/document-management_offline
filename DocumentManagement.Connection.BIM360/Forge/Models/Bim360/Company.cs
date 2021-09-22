using System;
using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
{
    [DataContract]
    public class Company
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "account_id")]
        public string AccountID { get; set; }

        [DataMember(Name = "project_id")]
        public string ProjectID { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "trade")]
        public string Trade { get; set; }

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

        [DataMember(Name = "website_url")]
        public string WebsiteUrl { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "created_at")]
        public DateTime CreatedAt { get; set; }

        [DataMember(Name = "updated_at")]
        public DateTime UpdatedAt { get; set; }

        [DataMember(Name = "erp_id")]
        public string ErpID { get; set; }

        [DataMember(Name = "tax_id")]
        public string TaxID { get; set; }

        [DataMember(Name = "member_group_id")]
        public string MemberGroupID { get; set; }
    }
}
