using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Authentication;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class AuthenticationService
    {
        private static readonly string SCOPE = "data:read%20data:write%20data:create";

        private readonly ForgeConnection connection;

        public AuthenticationService(ForgeConnection connection)
            => this.connection = connection;

        public string GetAuthorizationUri(string appPropertyClientId, string appPropertyCallBackUrl)
            => string.Format($"{FORGE_URL}{Resources.GetAuthorizeMethod}", appPropertyClientId, appPropertyCallBackUrl.Replace("/", "%2F"), SCOPE);

        public async Task<Token> GetTokenAsyncWithHttpInfo(string appPropertyClientId, string appPropertyClientSecret, string code, string appPropertyCallBackUrl)
        {
            HttpContent CreateContent()
                => new FormUrlEncodedContent(
                    new KeyValuePair<string, string>[]
                    {
                        new (AUTH_REQUEST_BODY_CLIENT_ID_FIELD, appPropertyClientId),
                        new (AUTH_REQUEST_BODY_CLIENT_SECRET_FIELD, appPropertyClientSecret),
                        new (AUTH_REQUEST_BODY_GRANT_TYPE_FIELD, AUTH_GRANT_TYPE_AUTHORIZATION_CODE_VALUE),
                        new (AUTH_REQUEST_BODY_CODE_FIELD, code),
                        new (AUTH_REQUEST_BODY_REDIRECT_URI_FIELD, appPropertyCallBackUrl),
                    });

            var data = await connection.SendAsync(
                    ForgeSettings.UnauthorizedPost(CreateContent),
                    Resources.PostGetTokenMethod);
            return data.ToObject<Token>();
        }

        public async Task<Token> RefreshTokenAsyncWithHttpInfo(string appPropertyClientId, string appPropertyClientSecret, string accessPropertyRefreshToken)
        {
            HttpContent CreateContent()
                => new FormUrlEncodedContent(
                    new KeyValuePair<string, string>[]
                    {
                        new (AUTH_REQUEST_BODY_CLIENT_ID_FIELD, appPropertyClientId),
                        new (AUTH_REQUEST_BODY_CLIENT_SECRET_FIELD, appPropertyClientSecret),
                        new (AUTH_REQUEST_BODY_GRANT_TYPE_FIELD, AUTH_GRANT_TYPE_REFRESH_TOKEN_VALUE),
                        new (AUTH_REQUEST_BODY_REFRESH_TOKEN_FIELD, accessPropertyRefreshToken),
                    });

            var data = await connection.SendAsync(
                    ForgeSettings.UnauthorizedPost(CreateContent),
                    Resources.PostRefreshTokenMethod);
            return data.ToObject<Token>();
        }

        public async Task<User> GetMe()
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                Resources.GetUsersMeMethod);
            return response.ToObject<User>();
        }
    }
}
