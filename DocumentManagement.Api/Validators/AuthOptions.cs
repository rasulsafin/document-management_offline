using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MRS.DocumentManagement.Api.Validators
{
    public class AuthOptions
    {
        public const string ISSUER = "BRIO.MRS.DM";
        public const string AUDIENCE = "BRIO.MRS";
        private const string KEY = "BRIO_MRS_SECURITY_KEY!1";

        public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}
