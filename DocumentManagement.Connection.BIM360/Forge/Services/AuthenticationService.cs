using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Authentication;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Authentication.Scopes;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class AuthenticationService
    {
        private static readonly Enum[] SCOPES =
        {
            DataScope.Read, DataScope.Write, DataScope.Create, AccountScope.Read,
        };

        private static readonly Enum[] APP_SCOPES = { AccountScope.Read };

        private readonly ForgeConnection connection;

        public AuthenticationService(ForgeConnection connection)
            => this.connection = connection;

        public string GetAuthorizationUri(string appPropertyClientId, string appPropertyCallBackUrl)
            => string.Format(
                $"{FORGE_URL}{Resources.GetAuthorizeMethod}",
                appPropertyClientId,
                appPropertyCallBackUrl.Replace("/", "%2F"),
                ScopeUtilities.GetScopeString(SCOPES));

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

        public async Task<Token> AuthenticateAppAsync(string appPropertyClientID, string appPropertyClientSecret)
        {
            HttpContent CreateContent()
                => new FormUrlEncodedContent(
                    new KeyValuePair<string, string>[]
                    {
                        new (AUTH_REQUEST_BODY_CLIENT_ID_FIELD, appPropertyClientID),
                        new (AUTH_REQUEST_BODY_CLIENT_SECRET_FIELD, appPropertyClientSecret),
                        new (AUTH_REQUEST_BODY_GRANT_TYPE_FIELD, AUTH_GRANT_TYPE_CLIENT_CREDENTIALS_VALUE),
                        new (AUTH_REQUEST_BODY_SCOPE_FIELD, ScopeUtilities.GetScopeString(APP_SCOPES)),
                    });

            var data = await connection.SendAsync(
                ForgeSettings.UnauthorizedPost(CreateContent),
                Resources.PostAuthenticateMethod);
            return data.ToObject<Token>();
        }

        public async Task<User> GetMeAsync()
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                Resources.GetUsersAtMeMethod);
            return response.ToObject<User>();
        }
    }
}
