using System;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.Authentication
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "emailId")]
        public string EmailId { get; set; }

        [DataMember(Name = "firstName")]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName")]
        public string LastName { get; set; }

        [DataMember(Name = "emailVerified")]
        public string EmailVerified { get; set; }

        [DataMember(Name = "2FaEnabled")]
        public string TwoFaEnabled { get; set; }

        [DataMember(Name = "countryCode")]
        public string CountryCode { get; set; }

        [DataMember(Name = "language")]
        public string Language { get; set; }

        [DataMember(Name = "optin")]
        public bool Optin { get; set; }

        [DataMember(Name = "lastModified")]
        public DateTime LastModified { get; set; }

        [DataMember(Name = "profileImages")]
        public ProfileImage Image { get; set; }

        [DataMember(Name = "websiteUrl")]
        public string WebsiteUrl { get; set; }

        [DataContract]
        public class ProfileImage
        {
            [DataMember(Name = "sizeX20")]
            public string SizeX20 { get; set; }

            [DataMember(Name = "sizeX40")]
            public string SizeX40 { get; set; }

            [DataMember(Name = "sizeX50")]
            public string SizeX50 { get; set; }

            [DataMember(Name = "sizeX58")]
            public string SizeX58 { get; set; }

            [DataMember(Name = "sizeX80")]
            public string SizeX80 { get; set; }

            [DataMember(Name = "sizeX120")]
            public string SizeX120 { get; set; }

            [DataMember(Name = "sizeX160")]
            public string SizeX160 { get; set; }

            [DataMember(Name = "sizeX176")]
            public string SizeX176 { get; set; }

            [DataMember(Name = "sizeX240")]
            public string SizeX240 { get; set; }

            [DataMember(Name = "sizeX360")]
            public string SizeX360 { get; set; }
        }
    }
}
