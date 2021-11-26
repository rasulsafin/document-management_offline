using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Authentication
{
    [DataContract]
    public class Token
    {
        [DataMember(Name = "token_type")]
        public string Type { get; set; }

        [DataMember(Name = "expires_in")]
        public int? ExpiresIn { get; set; }

        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
