using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MRS.Bim.DocumentManagement.Utilities;

namespace MRS.Bim.DocumentManagement.YandexDisk
{
    public class Auth : ICloudAuth
    {
        public AccessProperty AccessProperty { get; set; } = new AccessProperty("YandexDisk");
        public AppProperty appProperty { get; set; }
        public double timeout { get; set; } = 0.1;

        private delegate void NewBearerDelegate(dynamic bearer);

        private HttpListener httpListener;
        private DateTime sentTime;

        public Auth(string resource) => appProperty = AppProperty.LoadFromResources(resource);

        public async Task SignInAsync()
            => await CheckAccessAsync(true);

        public void ClearUserInfo()
        {
            AccessProperty.Token = "";
            AccessProperty.RefreshToken = "";
            AccessProperty.End = "";
        }

        public void Cancel()
        {
            httpListener?.Abort();
            httpListener = null;
        }

        private async Task CheckAccessAsync(bool mustUpdate = false)
        {
            await Task.Delay((int) (1000 * timeout));
            sentTime = DateTime.UtcNow;
            if ((!IsLogged || mustUpdate) && !await WebFeatures.RemoteUrlExistsAsync("https://yandex.ru/"))
                throw new Exception("Failed to ping the server");
            if (!IsLogged)
                await _3leggedAsync(GotIt);
        }

        private bool IsLogged
            => !string.IsNullOrEmpty(AccessProperty.End) &&
               DateTime.UtcNow < DateTime.Parse(AccessProperty.End);

        private void SaveData(dynamic bearer)
        {
            AccessProperty.Token = bearer.access_token;
            AccessProperty.End = sentTime.AddSeconds(bearer.expires_in).ToString();
        }

        private async Task _3leggedAsync(NewBearerDelegate cb)
        {
            Task<HttpListenerContext> getting = null;

            if (httpListener == null || !httpListener.IsListening)
            {
                if (!HttpListener.IsSupported)
                    return;
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(appProperty.callBackUrl.Replace("localhost", "+") + "/");
                httpListener.Start();
                getting = httpListener.GetContextAsync();
            }

            var oauthUrl = $"https://oauth.yandex.ru/authorize?response_type=token&client_id={appProperty.clientId}";
            Process.Start(oauthUrl);
            if (getting != null)
            {
                await getting;
                await RedirectAsync(getting.Result, GotIt);
            }
        }


        private async Task RedirectAsync(HttpListenerContext context, NewBearerDelegate callback)
        {
            try
            {
                var responseString =
                        "<html><head><script>function onLoad() { window.location.href = window.location.href.replace('#', '?') }</script></head><body onload=\"onLoad()\">...</body></html>";
                SetResponse(context, responseString);
                GetToken(await httpListener.GetContextAsync(), callback);
            }
            catch (Exception ex)
            {
                callback?.Invoke(null);
            }
            finally
            {
                httpListener.Stop();
            }
        }

        private void SetResponse(HttpListenerContext context, string responseString)
        {
            var buffer = Encoding.UTF8.GetBytes(responseString);
            var response = context.Response;
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        private void GetToken(HttpListenerContext context, NewBearerDelegate callback)
        {
            dynamic bearer = new
            {
                access_token = context.Request.QueryString["access_token"],
                token_type = context.Request.QueryString["token_type"],
                expires_in = int.Parse(context.Request.QueryString["expires_in"])
            };

            var responseString = "<html><body>You can now close this window!</body></html>";
            SetResponse(context, responseString);
            GotIt(bearer);
        }

        private void GotIt(dynamic bearer)
        {
            if (bearer == null)
                throw new Exception("Sorry, Authentication failed");

            SaveData(bearer);
        }
    }
}