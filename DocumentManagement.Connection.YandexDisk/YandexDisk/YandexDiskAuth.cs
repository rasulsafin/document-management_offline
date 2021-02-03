using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexDiskAuth
    {
        public static readonly string CLIENT_ID = "b1a5acbc911b4b31bc68673169f57051";
        private static readonly string CLIENT_SECRET = "b4890ed3aa4e4a4e9e207467cd4a0f2c";
        private static readonly string RETURN_URL = @"http://localhost:5000/Authorizations/yandex-disk";
        public static readonly string OAUTH_URL = $"https://oauth.yandex.ru/authorize?response_type=token&client_id={CLIENT_ID}";
    }
}
