using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexDiskAuth
    {
        public string access_token;
        public string token_type;
        public int expires_in;

        private static readonly string CLIENT_ID = "b1a5acbc911b4b31bc68673169f57051";
        private static readonly string CLIENT_SECRET = "b4890ed3aa4e4a4e9e207467cd4a0f2c";
        private static readonly string RETURN_URL = @"http://localhost:8000/oauth/";

        /// <summary>
        /// https://yandex.ru/dev/oauth/doc/dg/reference/auto-code-client.html.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<string> GetYandexDiskToken()
        {
            if (!HttpListener.IsSupported)
                throw new Exception("The listener is not supported.");
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add(RETURN_URL);
            httpListener.Start();

            // stage 1, 2, 3
            // var oauthUrl = $"https://oauth.yandex.ru/authorize?response_type=token&client_id={CLIENT_ID}";
            var oauthUrl = $"https://oauth.yandex.ru/authorize?response_type=token&client_id={CLIENT_ID}";

            YandexHelper.OpenBrowser(oauthUrl);
            string result = string.Empty;

            try
            {
                // stage 4
                HttpListenerContext context = await httpListener.GetContextAsync();
                if (context != null)
                {
                    var responseStage5 = @"<!doctype html>
<html><head>
<script>function onLoad() { window.location.href = window.location.href.replace('#', '?') }</script>
</head>
<body onload=""onLoad()"">...</body>
</html>";

                    // stage 5
                    SetResponse(context, responseStage5);
                }

                // stage 6
                context = await httpListener.GetContextAsync();
                access_token = context.Request.QueryString["access_token"];
                token_type = context.Request.QueryString["token_type"];
                expires_in = int.Parse(context.Request.QueryString["expires_in"]);

                var responseString = @"<html>
<script>function onLoad()
{
window.close();
}</script>
<body onload=""onLoad()"">You can now close this window!</body></html>";
                SetResponse(context, responseString);

                // === complete ===
                result = access_token;

                // ===          ===
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                httpListener.Stop();
            }

            return result;
        }

        private void SetResponse(HttpListenerContext context, string responseString)
        {
            var buffer = Encoding.UTF8.GetBytes(responseString);
            var response = context.Response;
            response.ContentType = "text/html; charset utf-8";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}
