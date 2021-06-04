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

        internal async Task<ConnectionStatusDto> Connect(string email, string password)
        {
            try
            {
                var userToLogin = new LoginUser()
                {
                    Email = email,
                    Password = password,
                };

                var userWithToken = await Connector.SendAsyncJson<UserWithToken, LoginUser>(HttpMethod.Post, URLs.PostLogin, userToLogin);
                MrsProHttpConnection.Token = userWithToken.AccessToken;

                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.OK, Message = "Connection complete." };
            }
            catch (Exception ex)
            {
                return new ConnectionStatusDto() { Status = RemoteConnectionStatus.Error, Message = ex.Message };
            }
        }
    }
}
