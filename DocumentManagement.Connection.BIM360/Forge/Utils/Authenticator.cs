using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.Internal;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Authentication;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class Authenticator : IDisposable
    {
        private const string RESPONSE_HTML_TYPE = "text/html";
        private static readonly string SUCCESSFUL_AUTHENTICATION_PAGE = "SuccessfulAuthentication";

        private readonly AuthenticationService service;
        private HttpListener httpListener;
        private DateTime sentTime;

        // Is created as scoped as this service
        private ConnectionInfoDto connectionInfoDto;

        public Authenticator(AuthenticationService service)
            => this.service = service;

        internal delegate void NewBearerDelegate(Token bearer);

        public enum ConnectionStatus
        {
            NotConnected,
            Connecting,
            Connected,
            Error,
        }

        public ConnectionStatus Status { get; private set; }

        public bool IsLogged
            => !string.IsNullOrEmpty(AccessEnd) && DateTime.UtcNow < DateTime.Parse(AccessEnd);

        private string AccessToken
        {
            get => connectionInfoDto.AuthFieldValues?.GetOrDefault(TOKEN_AUTH_NAME);
            set => SetAuthValue(connectionInfoDto, TOKEN_AUTH_NAME, value);
        }

        private string AccessRefreshToken
        {
            get => connectionInfoDto.AuthFieldValues?.GetOrDefault(REFRESH_TOKEN_AUTH_NAME);
            set => SetAuthValue(connectionInfoDto, REFRESH_TOKEN_AUTH_NAME, value);
        }

        private string AccessEnd
        {
            get => connectionInfoDto.AuthFieldValues?.GetOrDefault(END_AUTH_NAME);
            set => SetAuthValue(connectionInfoDto, END_AUTH_NAME, value);
        }

        private string AppClientId
        {
            get => connectionInfoDto.ConnectionType.AppProperties.GetOrDefault(CLIENT_ID_NAME);
            set => SetAppProperty(connectionInfoDto, CLIENT_ID_NAME, value);
        }

        private string AppClientSecret
        {
            get => connectionInfoDto.ConnectionType.AppProperties.GetOrDefault(CLIENT_SECRET_NAME);
            set => SetAppProperty(connectionInfoDto, CLIENT_SECRET_NAME, value);
        }

        private string AppCallBackUrl
        {
            get => connectionInfoDto.ConnectionType.AppProperties.GetOrDefault(CALLBACK_URL_NAME);
            set => SetAppProperty(connectionInfoDto, CALLBACK_URL_NAME, value);
        }

        public void Dispose()
        {
            httpListener.Close();
            GC.SuppressFinalize(this);
        }

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

        public async Task<(ConnectionStatusDto authStatus, ConnectionInfoDto updatedInfo)> SignInAsync(ConnectionInfoDto connectionInfo)
        {
            connectionInfoDto = connectionInfo;
            await CheckAccessAsync(true);

            // TODO Add filling connection status depending on 'status' field
            var result = new ConnectionStatusDto { Status = RemoteConnectionStatusDto.OK };

            return (result, connectionInfo);
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
                Status = ConnectionStatus.Connecting;
                var bearer = await service.RefreshTokenAsyncWithHttpInfo(AppClientId, AppClientSecret, AccessRefreshToken);
                SaveData(bearer);
            }
            catch
            {
            }
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

                var oAuthUri = service.GetAuthorizationUri(AppClientId, AppCallBackUrl);
                Process.Start(new ProcessStartInfo(oAuthUri) { UseShellExecute = true });

                if (getting != null)
                {
                    await getting;
                    await ThreeLeggedWaitForCodeAsync(getting.Result, GotIt);
                }
            }
            catch (Exception)
            {
                Status = ConnectionStatus.Error;
                throw;
            }
        }

        internal async Task ThreeLeggedWaitForCodeAsync(HttpListenerContext context, NewBearerDelegate callback)
        {
            try
            {
                var code = context.Request.QueryString[CODE_QUERY_KEY];
                var responseString = (string)Resources.ResourceManager.GetObject(SUCCESSFUL_AUTHENTICATION_PAGE);
                var buffer = Encoding.UTF8.GetBytes(responseString ?? string.Empty);
                var response = context.Response;
                response.ContentType = RESPONSE_HTML_TYPE;
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;
                await response.OutputStream.WriteAsync(buffer.AsMemory());
                response.OutputStream.Close();

                if (!string.IsNullOrEmpty(code))
                {
                    var bearer =
                            await service.GetTokenAsyncWithHttpInfo(AppClientId, AppClientSecret, code, AppCallBackUrl);
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

        private string GetAuthOrDefault(IEnumerable<string> source, int index)
            => index != -1 ? source.ElementAtOrDefault(index) : string.Empty;

        private void SaveData(Token bearer)
        {
            AccessToken = bearer.AccessToken;
            AccessRefreshToken = bearer.RefreshToken;
            AccessEnd = sentTime.AddSeconds(bearer.ExpiresIn ?? 0).ToString();
            Status = ConnectionStatus.Connected;
        }

        private void GotIt(Token bearer)
        {
            if (bearer == null)
            {
                Status = ConnectionStatus.Error;
                throw new Exception("Sorry, Authentication failed");
            }

            SaveData(bearer);
        }

        private void SetAuthValue(ConnectionInfoDto info, string key, string value)
        {
            info.AuthFieldValues ??= new Dictionary<string, string>();
            SetDictionaryValue(info.AuthFieldValues, key, value);
        }

        private void SetAppProperty(ConnectionInfoDto info, string key, string value)
        {
            info.ConnectionType.AppProperties ??= new Dictionary<string, string>();
            SetDictionaryValue(info.ConnectionType.AppProperties, key, value);
        }

        private void SetDictionaryValue(IDictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }

            dictionary.Add(key, value);
        }
    }
}
