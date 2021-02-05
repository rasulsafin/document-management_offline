using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudApis.Utils;
using DocumentManagement.Connection.BIM360.Properties;
using Forge.Models.Authentication;
using Newtonsoft.Json;

namespace Forge.Services
{
    public class AuthenticationService : IDisposable
    {
        public ConnectionStatus status;

        private static readonly double TIMEOUT = 10;
        private static readonly string SCOPE = "data:read%20data:write%20data:create";

        private static HttpClient client;

        private HttpListener httpListener;
        private DateTime sentTime;
        private IntPtr currentProcess;

        private AuthenticationService()
        {
            client = new HttpClient { Timeout = TimeSpan.FromSeconds(TIMEOUT) };
        }

        internal delegate void NewBearerDelegate(Token bearer);

        public enum ConnectionStatus
        {
            NotConnected,
            Connecting,
            Connected,
            Error,
        }

        public AccessProperty AccessProperty { get; set; }

        public AppProperty AppProperty { get; set; }

        public bool IsLogged => !string.IsNullOrEmpty(AccessProperty.End) && DateTime.UtcNow < DateTime.Parse(AccessProperty.End);

        public async Task CheckAccessAsync(bool mustUpdate = false)
        {
            sentTime = DateTime.UtcNow;
            if (!IsLogged || mustUpdate)
            {
                if (string.IsNullOrEmpty(AccessProperty.Token))
                    await ThreeLeggedAsync();
                else
                    await RefreshConnectionAsync();
            }
        }

        public async Task SignInAsync()
        {
           await CheckAccessAsync(true);
        }

        public void Cancel()
        {
            httpListener?.Abort();
            httpListener = null;
        }

        public void ClearUserInfo()
        {
            AccessProperty.Token = string.Empty;
            AccessProperty.RefreshToken = string.Empty;
            AccessProperty.End = string.Empty;
        }

        public async Task RefreshConnectionAsync()
        {
            try
            {
                status = ConnectionStatus.Connecting;
                var bearer = await RefreshTokenAsyncWithHttpInfo(AppProperty.clientId,
                        AppProperty.clientSecret,
                        AccessProperty.RefreshToken);
                SaveData(bearer);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }

        internal async Task ThreeLeggedAsync()
        {
            try
            {
                if (!await WebFeatures.RemoteUrlExistsAsync("https://autodesk.com/"))
                    throw new Exception("Failed to ping the server");
                Task<HttpListenerContext> getting = null;
                if (httpListener == null || !httpListener.IsListening)
                {
                    if (!HttpListener.IsSupported)
                        return;
                    httpListener = new HttpListener();
                    httpListener.Prefixes.Add(AppProperty.callBackUrl.Replace("localhost", "+") + "/");
                    httpListener.Start();
                    getting = httpListener.GetContextAsync();
                }

                var oauthUrl = Authorize(AppProperty.clientId, AppProperty.callBackUrl);
                Process.Start(oauthUrl);
                if (getting != null)
                    await getting;
                await ThreeLeggedWaitForCodeAsync(getting.Result, GotIt);
            }
            catch (Exception ex)
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
                    var bearer = await GetTokenAsyncWithHttpInfo(AppProperty.clientId, AppProperty.clientSecret, code, AppProperty.callBackUrl);
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

        private async Task<Token> RefreshTokenAsyncWithHttpInfo(string appProperyClientId, string appPropertyClientSecret, string accessPropertyRefreshToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v1/refreshtoken")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", appProperyClientId),
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
            AccessProperty.Token = bearer.AccessToken;
            AccessProperty.RefreshToken = bearer.RefreshToken;
            AccessProperty.End = sentTime.AddSeconds(bearer.ExpiresIn).ToString();
            status = ConnectionStatus.Connected;
        }

        private string Authorize(string appPropertyClientId, string appPropertyCallBackUrl)
            => $"https://developer.api.autodesk.com/authentication/v1/refreshtoken?response_type=code&client_id={appPropertyClientId}&redirect_uri={appPropertyCallBackUrl}&scope={SCOPE}";

        private async Task<Token> GetTokenAsyncWithHttpInfo(string appProperyClientId, string appProperyClientSecret, string code, string appProperyCallBackUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v1/gettoken")
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
