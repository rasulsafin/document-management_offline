using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils
{
    public class ForgeSettings
    {
        private ForgeSettings()
        {
        }

        public HttpMethod MethodType { get; private set; }

        public HttpContent Content { get; private set; }

        public Stream Stream { get; private set; }

        public object Data { get; private set; }

        public object Included { get; private set; }

        public bool NeedJsonApi { get; private set; } = false;

        public bool IsAuthorized { get; private set; } = true;

        public bool NeedDataKey { get; private set; } = true;

        public RangeHeaderValue RangeHeaderValue { get; private set; }

        public static ForgeSettings UnauthorizedPost(HttpContent content)
            => new ForgeSettings
            {
                MethodType = HttpMethod.Post,
                Content = content,
                IsAuthorized = false,
            };

        public static ForgeSettings AuthorizedGet()
            => new ForgeSettings
            {
                MethodType = HttpMethod.Get,
            };

        public static ForgeSettings AuthorizedDelete()
            => new ForgeSettings
            {
                MethodType = HttpMethod.Delete,
            };

        public static ForgeSettings AuthorizedPost(object data)
            => new ForgeSettings
            {
                MethodType = HttpMethod.Post,
                Data = data,
            };

        public static ForgeSettings AuthorizedPostWithoutKeyData(object data)
            => new ForgeSettings
            {
                MethodType = HttpMethod.Post,
                Data = data,
                NeedDataKey = false,
            };

        public static ForgeSettings AuthorizedPostWithJsonApi(object data)
            => new ForgeSettings
            {
                MethodType = HttpMethod.Post,
                Data = data,
                NeedJsonApi = true,
            };

        public static ForgeSettings AuthorizedPostWithJsonApi(object data, object included)
            => new ForgeSettings
            {
                MethodType = HttpMethod.Post,
                Data = data,
                Included = included,
                NeedJsonApi = true,
            };

        public static ForgeSettings AuthorizedPut(Stream stream)
            => new ForgeSettings
            {
                MethodType = HttpMethod.Put,
                Stream = stream,
            };

        public static ForgeSettings AuthorizedPut(Stream stream, RangeHeaderValue range)
            => new ForgeSettings
            {
                MethodType = HttpMethod.Put,
                Stream = stream,
                RangeHeaderValue = range,
            };

        public static ForgeSettings AuthorizedPatch(object data)
            => new ForgeSettings
            {
                MethodType = HttpMethod.Patch,
                Data = data,
            };
    }
}
