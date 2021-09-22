using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Client.Dtos;
using System;
using System.Threading.Tasks;

namespace Brio.Docs.Connections.MrsPro.Services
{
    public class AuthenticationService : Service
    {
        private static readonly string BASE_URL = "/login";

        public AuthenticationService(MrsProHttpConnection connection)
            : base(connection) { }

        internal async Task<ConnectionStatusDto> SignInAsync(string email, string password, string companyCode)
        {
            try
            {
                Auth.CompanyCode = companyCode;

                var userToLogin = new Login()
                {
                    Email = email,
                    Password = password,
                };

                var userWithToken = await HttpConnection.PostJson<User, Login>(BASE_URL, userToLogin);
                if (userWithToken.AccessToken == null)
                    throw new Exception("No connection.");

                Auth.Token = userWithToken.AccessToken;
                Auth.OrganizationId = userWithToken.OrganizationId;
                Auth.UserId = userWithToken.Id;

                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK, Message = "Connection complete." };
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = ex.Message };
            }
        }

        internal async Task<ConnectionStatusDto> TryPing()
        {
            try
            {
                await HttpConnection.Get<object>("/ping/user");
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK, Message = "Connection is stable." };
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.NeedReconnect, Message = ex.Message };
            }
        }
    }
}
