using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
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

                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK, Message = "Connection complete." };
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = ex.Message };
            }
        }
    }
}
