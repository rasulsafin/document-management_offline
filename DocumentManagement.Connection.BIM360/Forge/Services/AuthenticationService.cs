using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge.Models.Authentication;
using DocumentManagement.Connection.BIM360.Properties;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using static DocumentManagement.Connection.BIM360.Forge.Constants;

namespace DocumentManagement.Connection.BIM360.Forge.Services
{
    public class AuthenticationService : IDisposable
    {
        public ConnectionStatus status;

        private static readonly double TIMEOUT = 10;
        private static readonly string SCOPE = "data:read%20data:write%20data:create";

        private HttpClient client;
        private HttpListener httpListener;
        private DateTime sentTime;
        private IntPtr currentProcess;

        // Is created as scoped as this service
        private RemoteConnectionInfoDto connectionInfoDto;

        private AuthenticationService()
            => client = new HttpClient { Timeout = TimeSpan.FromSeconds(TIMEOUT) };

        internal delegate void NewBearerDelegate(Token bearer);

        public enum ConnectionStatus
        {
            NotConnected,
            Connecting,
            Connected,
            Error,
        }

        private string AccessToken
        {
            get
            {
                var authNamesList = connectionInfoDto.ConnectionType.AuthFieldNames.ToList();
                var authValuesSource = connectionInfoDto.AuthFieldValues;
                var tokenIndex = FindAuthIndex(authNamesList, TOKEN_AUTH_NAME);
                return GetAuthOrDefault(authValuesSource, tokenIndex);
            }

            set
            {
                var authNamesList = connectionInfoDto.ConnectionType.AuthFieldNames.ToList();
                var authValuesSource = connectionInfoDto.AuthFieldValues;
                var tokenIndex = FindAuthIndex(authNamesList, TOKEN_AUTH_NAME);
                var updatedValues = authValuesSource.ToList();
                updatedValues[tokenIndex] = value;
                connectionInfoDto.AuthFieldValues = updatedValues;
            }
        }

        private string AccessRefreshToken
        {
            get
            {
                var authNamesList = connectionInfoDto.ConnectionType.AuthFieldNames.ToList();
                var authValuesSource = connectionInfoDto.AuthFieldValues;
                var refreshTokenIndex = FindAuthIndex(authNamesList, REFRESH_TOKEN_AUTH_NAME);
                return GetAuthOrDefault(authValuesSource, refreshTokenIndex);
            }

            set
            {
                var authNamesList = connectionInfoDto.ConnectionType.AuthFieldNames.ToList();
                var authValuesSource = connectionInfoDto.AuthFieldValues;
                var refreshTokenIndex = FindAuthIndex(authNamesList, REFRESH_TOKEN_AUTH_NAME);
                var updatedValues = authValuesSource.ToList();
                updatedValues[refreshTokenIndex] = value;
                connectionInfoDto.AuthFieldValues = updatedValues;
            }
        }

        private string AccessEnd
        {
            get
            {
                var authNamesList = connectionInfoDto.ConnectionType.AuthFieldNames.ToList();
                var authValuesSource = connectionInfoDto.AuthFieldValues;
                var endIndex = FindAuthIndex(authNamesList, END_AUTH_NAME);
                return GetAuthOrDefault(authValuesSource, endIndex);
            }

            set
            {
                var authNamesList = connectionInfoDto.ConnectionType.AuthFieldNames.ToList();
                var authValuesSource = connectionInfoDto.AuthFieldValues;
                var endIndex = FindAuthIndex(authNamesList, END_AUTH_NAME);
                var updatedValues = authValuesSource.ToList();
                updatedValues[endIndex] = value;
                connectionInfoDto.AuthFieldValues = updatedValues;
            }
        }

        private string AppClientId
        {
            get => connectionInfoDto.ConnectionType.AppProperty[CLIENT_ID_NAME];
            set => connectionInfoDto.ConnectionType.AppProperty[CLIENT_ID_NAME] = value;
        }

        private string AppClientSecret
        {
            get => connectionInfoDto.ConnectionType.AppProperty[CLIENT_SECRET_NAME];
            set => connectionInfoDto.ConnectionType.AppProperty[CLIENT_SECRET_NAME] = value;
        }

        private string AppCallBackUrl
        {
            get => connectionInfoDto.ConnectionType.AppProperty[CALLBACK_URL_NAME];
            set => connectionInfoDto.ConnectionType.AppProperty[CALLBACK_URL_NAME] = value;
        }

        public bool IsLogged
            => !string.IsNullOrEmpty(AccessEnd) && DateTime.UtcNow < DateTime.Parse(AccessEnd);

        public async Task CheckAccessAsync(bool mustUpdate = false)
        {
            sentTime = DateTime.UtcNow;
            if (!IsLogged || mustUpdate)
            {
                if (string.IsNullOrEmpty(AccessToken))
                    await ThreeLeggedAsync();
                else
                    await RefreshConnectionAsync();
            }
        }

        public async Task<ConnectionStatusDto> SignInAsync(RemoteConnectionInfoDto connectionInfo)
        {
            connectionInfoDto = connectionInfo;
            await CheckAccessAsync(true);

            // TODO Add filling connection status depending on 'status' field
            var result = new ConnectionStatusDto();

            return result;
        }

        public void Cancel()
        {
            httpListener?.Abort();
            httpListener = null;
        }

        public async Task RefreshConnectionAsync()
        {
            try
            {
                status = ConnectionStatus.Connecting;
                var bearer = await RefreshTokenAsyncWithHttpInfo(AppClientId,
                        AppClientSecret,
                        AccessRefreshToken);
                SaveData(bearer);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            client.Dispose();
            ((IDisposable)httpListener).Dispose();
        }

        internal async Task ThreeLeggedAsync()
        {
            try
            {
                if (!await WebFeatures.RemoteUrlExistsAsync(Resources.AutodeskUrl))
                    throw new Exception("Failed to ping the server");
                Task<HttpListenerContext> getting = null;
                if (httpListener == null || !httpListener.IsListening)
                {
                    if (!HttpListener.IsSupported)
                        return;
                    httpListener = new HttpListener();
                    httpListener.Prefixes.Add(AppCallBackUrl/*.Replace("localhost", "+") + "/"*/);
                    httpListener.Start();
                    getting = httpListener.GetContextAsync();
                }

                var oauthUrl = Authorize(AppClientId, AppCallBackUrl);
                Process.Start(new ProcessStartInfo(oauthUrl) { UseShellExecute = true });
                if (getting != null)
                    await getting;
                await ThreeLeggedWaitForCodeAsync(getting.Result, GotIt);
            }
            catch (Exception)
            {
                status = ConnectionStatus.Error;
                throw;
            }
        }

        internal async Task ThreeLeggedWaitForCodeAsync(HttpListenerContext context, NewBearerDelegate callback)
        {
            try
            {
                var code = context.Request.QueryString["code"];
                var responseString = (string)Resources.ResourceManager.GetObject("SuccessfulAuthentication");
                var buffer = Encoding.UTF8.GetBytes(responseString ?? string.Empty);
                var response = context.Response;
                response.ContentType = "text/html";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;
                await response.OutputStream.WriteAsync(buffer.AsMemory());
                response.OutputStream.Close();

                if (!string.IsNullOrEmpty(code))
                {
                    var bearer = await GetTokenAsyncWithHttpInfo(AppClientId, AppClientSecret, code, AppCallBackUrl);
                    callback?.Invoke(bearer);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            catch (Exception)
            {
                callback?.Invoke(null);
            }
            finally
            {
                httpListener.Stop();
            }
        }

        private int FindAuthIndex(List<string> source, string authName)
            => source.FindIndex(a => a.Equals(authName, StringComparison.InvariantCultureIgnoreCase));

        private string GetAuthOrDefault(IEnumerable<string> source, int index)
            => index != -1 ? source.ElementAtOrDefault(index) : string.Empty;

        private async Task<Token> RefreshTokenAsyncWithHttpInfo(string appPropertyClientId, string appPropertyClientSecret, string accessPropertyRefreshToken)
        {
            var url = new StringBuilder(Resources.ForgeUrl).Append(Resources.PostRefreshTokenMethod).ToString();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", appPropertyClientId),
                    new KeyValuePair<string, string>("client_secret", appPropertyClientSecret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", accessPropertyRefreshToken),
                }),
            };
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Token>(data);
        }

        private void SaveData(Token bearer)
        {
            AccessToken = bearer.AccessToken;
            AccessRefreshToken = bearer.RefreshToken;
            AccessEnd = sentTime.AddSeconds(bearer.ExpiresIn).ToString();
            status = ConnectionStatus.Connected;
        }

        private string Authorize(string appPropertyClientId, string appPropertyCallBackUrl)
            => string.Format($"{Resources.ForgeUrl}{Resources.GetAuthorizeMethod}", appPropertyClientId, appPropertyCallBackUrl.Replace("/", "%2F"), SCOPE);

        private async Task<Token> GetTokenAsyncWithHttpInfo(string appProperyClientId, string appProperyClientSecret, string code, string appProperyCallBackUrl)
        {
            var url = new StringBuilder(Resources.ForgeUrl).Append(Resources.PostGetTokenMethod).ToString();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", appProperyClientId),
                    new KeyValuePair<string, string>("client_secret", appProperyClientSecret),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", appProperyCallBackUrl),
                }),
            };
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Token>(data);
        }

        private void GotIt(Token bearer)
        {
            if (bearer == null)
            {
                status = ConnectionStatus.Error;
                throw new Exception("Sorry, Authentication failed");
            }

            SaveData(bearer);
        }
    }
}
