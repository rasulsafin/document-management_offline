using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    [DataContract]
    public class Login
    {
        [DataMember(Name = "authToken")]
        public string AuthToken { get; set; }

        [DataMember(Name = "clientToken")]
        public string ClientToken { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "reCaptchaToken")]
        public string ReCaptchaToken { get; set; }
    }
}
