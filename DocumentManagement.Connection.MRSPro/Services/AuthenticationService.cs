using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Properties;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class AuthenticationService : Service
    {
        public AuthenticationService(MrsProHttpConnection connection)
            : base(connection) { }

        internal async Task<string> Connect(string email, string password, string companyCode)
        {
            Auth.CompanyCode = companyCode;

            var userToLogin = new LoginUser()
            {
                Email = email,
                Password = password,
            };

            var userWithToken = await HttpConnection.SendAsyncJson<UserWithToken, LoginUser>(HttpMethod.Post, URLs.PostLogin, userToLogin);
            Auth.Token = userWithToken.AccessToken;
            Auth.OrganizationId = userWithToken.OrganizationId;

            return userWithToken.Id;
        }
    }
}
