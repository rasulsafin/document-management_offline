using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.LementPro.Models
{
    public class AuthorizationData
    {
        [DataMember(Name = "loginName")]
        public string LoginName { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "rememberMe")]
        public bool? RememberMe { get; set; }

        [DataMember(Name = "returnUrl")]
        public string ReturnUrl { get; set; }
    }
}
