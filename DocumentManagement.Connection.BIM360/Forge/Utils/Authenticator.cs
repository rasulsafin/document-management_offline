using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Authentication;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using MRS.DocumentManagement.Connection.Utils.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class Authenticator : IDisposable
    {
        private const string RESPONSE_HTML_TYPE = "text/html";
        private static readonly string SUCCESSFUL_AUTHENTICATION_PAGE = "SuccessfulAuthentication";
        private static readonly string AUTODESK_WEBSITE = "https://autodesk.com/";

        private readonly AuthenticationService service;
        private readonly ILogger<Authenticator> logger;
        private HttpListener httpListener;
        private DateTime sentTime;

        // Is created as scoped as this service
        private ConnectionInfoExternalDto connectionInfo;

        public Authenticator(AuthenticationService service, ILogger<Authenticator> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        public enum ConnectionStatus
        {
            NotConnected,
            Connecting,
            Connected,
            Error,
        }

        private ConnectionStatus Status { get; set; }

        private bool IsLogged => !string.IsNullOrEmpty(AccessEnd) && DateTime.UtcNow < DateTime.Parse(AccessEnd, CultureInfo.InvariantCulture);

        private string AccessToken
        {
            get => connectionInfo.GetAuthValue(TOKEN_AUTH_NAME);
            set => connectionInfo.SetAuthValue(TOKEN_AUTH_NAME, value);
        }

        private string AccessRefreshToken
        {
            get => connectionInfo.GetAuthValue(REFRESH_TOKEN_AUTH_NAME);
            set => connectionInfo.SetAuthValue(REFRESH_TOKEN_AUTH_NAME, value);
        }

        private string AccessEnd
        {
            get => connectionInfo.GetAuthValue(END_AUTH_NAME);
            set => connectionInfo.SetAuthValue(END_AUTH_NAME, value);
        }

        private string AppClientId => connectionInfo.GetAppProperty(CLIENT_ID_NAME);

        private string AppClientSecret => connectionInfo.GetAppProperty(CLIENT_SECRET_NAME);

        private string AppCallBackUrl => connectionInfo.GetAppProperty(CALLBACK_URL_NAME);

        public void Dispose()
        {
            httpListener?.Close();
            GC.SuppressFinalize(this);
        }

        public async Task<(ConnectionStatusDto authStatus, ConnectionInfoExternalDto updatedInfo)> SignInAsync(
            ConnectionInfoExternalDto connectionInfo,
            CancellationToken token = default)
        {
            this.connectionInfo = connectionInfo;
            await CheckAccessAsync(token, true);

            var result = new ConnectionStatusDto
            {
                Status = Status switch
                {
                    ConnectionStatus.Connected => RemoteConnectionStatus.OK,
                    _ => RemoteConnectionStatus.Error
                },
            };

            return (result, connectionInfo);
        }

        private void Cancel()
        {
            httpListener.Abort();
            httpListener = null;
        }

        private async Task CheckAccessAsync(CancellationToken token, bool mustUpdate = false)
        {
            sentTime = DateTime.UtcNow;

            if (!IsLogged || mustUpdate)
            {
                if (string.IsNullOrEmpty(AccessToken))
                {
                    await SignInAsync(token);
                }
                else
                {
                    try
                    {
                        await RefreshConnectionAsync();
                    }
                    catch
                    {
                        await SignInAsync(token);
                    }
                }
            }
        }

        private async Task RefreshConnectionAsync()
        {
            try
            {
                Status = ConnectionStatus.Connecting;
                var bearer = await service.RefreshTokenAsyncWithHttpInfo(
                    AppClientId,
                    AppClientSecret,
                    AccessRefreshToken);
                SaveData(bearer);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't refresh connection");
            }
        }

        private async Task SignInAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (!await WebFeatures.RemoteUrlExistsAsync(AUTODESK_WEBSITE))
                    throw new Exception("Failed to ping the server");

                Task<HttpListenerContext> getting = null;

                if (httpListener == null || !httpListener.IsListening)
                    getting = StartListening(token);

                token.ThrowIfCancellationRequested();
                OpenBrowser();

                token.ThrowIfCancellationRequested();

                if (getting != null)
                {
                    await Wait(getting, token);

                    try
                    {
                        var code = getting.Result.Request.QueryString[CODE_QUERY_KEY];
                        await OpenResultPage(getting.Result);
                        if (code == null)
                            throw new Exception("Authentication failed");

                        var authToken = await service.GetTokenAsyncWithHttpInfo(
                            AppClientId,
                            AppClientSecret,
                            code,
                            AppCallBackUrl);
                        SaveData(authToken);
                    }
                    finally
                    {
                        httpListener.Stop();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Cancel();
                throw;
            }
            catch (Exception)
            {
                Status = ConnectionStatus.Error;
                throw;
            }
        }

        private void OpenBrowser()
        {
            var oAuthUri = service.GetAuthorizationUri(AppClientId, AppCallBackUrl);
            Process.Start(new ProcessStartInfo(oAuthUri) { UseShellExecute = true });
        }

        private Task<HttpListenerContext> StartListening(CancellationToken token)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Not supported on this machine");

            httpListener = new HttpListener();
            httpListener.Prefixes.Add(AppCallBackUrl);
            httpListener.Start();
            token.ThrowIfCancellationRequested();
            return httpListener.GetContextAsync();
        }

        private async Task Wait(IAsyncResult getting, CancellationToken token)
        {
            while (!getting.IsCompleted)
            {
                await Task.Delay(100, token);
                token.ThrowIfCancellationRequested();
            }
        }

        private async Task OpenResultPage(HttpListenerContext context)
        {
            var responseString = (string)Resources.ResourceManager.GetObject(SUCCESSFUL_AUTHENTICATION_PAGE);
            var buffer = Encoding.UTF8.GetBytes(responseString ?? string.Empty);
            var response = context.Response;
            response.ContentType = RESPONSE_HTML_TYPE;
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            await response.OutputStream.WriteAsync(buffer.AsMemory());
            response.OutputStream.Close();
        }

        private void SaveData(Token bearer)
        {
            AccessToken = bearer.AccessToken;
            AccessRefreshToken = bearer.RefreshToken;
            AccessEnd = sentTime.AddSeconds(bearer.ExpiresIn ?? 0).ToString(CultureInfo.InvariantCulture);
            Status = ConnectionStatus.Connected;
        }
    }
}
