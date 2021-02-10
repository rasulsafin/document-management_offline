using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge.Models.Authentication;
using DocumentManagement.Connection.BIM360.Forge.Services;
using DocumentManagement.Connection.BIM360.Properties;
using MRS.DocumentManagement.Interface.Dtos;
using static DocumentManagement.Connection.BIM360.Forge.Constants;

namespace DocumentManagement.Connection.BIM360.Forge.Utils
{
    public class Authenticator
    {
        public ConnectionStatus status;

        private readonly AuthenticationService service;
        private HttpListener httpListener;
        private DateTime sentTime;

        // Is created as scoped as this service
        private RemoteConnectionInfoDto connectionInfoDto;

        private Authenticator(AuthenticationService service)
            => this.service = service;

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

        private int FindAuthIndex(List<string> source, string authName)
            => source.FindIndex(a => a.Equals(authName, StringComparison.InvariantCultureIgnoreCase));

        private string GetAuthOrDefault(IEnumerable<string> source, int index)
            => index != -1 ? source.ElementAtOrDefault(index) : string.Empty;

        private void SaveData(Token bearer)
        {
            AccessToken = bearer.AccessToken;
            AccessRefreshToken = bearer.RefreshToken;
            AccessEnd = sentTime.AddSeconds(bearer.ExpiresIn).ToString();
            status = ConnectionStatus.Connected;
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
