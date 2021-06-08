using System;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Properties;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class AuthenticationService : Service
    {
        public AuthenticationService(MrsProHttpConnection connection)
            : base(connection) { }

        public async Task<(string userId, ConnectionStatusDto authStatus)> SignInAsync(string email, string password, string companyCode)
        {
            try
            {
                Auth.CompanyCode = companyCode;

                var userToLogin = new LoginUser()
                {
                    Email = email,
                    Password = password,
                };

                var userWithToken = await HttpConnection.SendAsyncJson<UserWithToken, LoginUser>(HttpMethod.Post, URLs.PostLogin, userToLogin);
                if (userWithToken.AccessToken == null)
                    throw new Exception("No connection.");

                Auth.Token = userWithToken.AccessToken;
                Auth.OrganizationId = userWithToken.OrganizationId;

                return (userWithToken.Id, new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK, Message = "Connection complete." });
            }
            catch (Exception ex)
            {
                return (null, new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = ex.Message });
            }
        }
    }
}
