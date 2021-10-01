using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Brio.Docs.Connections.Bim360.Forge.Utils
{
    public class ForgeSettings
    {
        private ForgeSettings()
        {
        }

        public HttpMethod MethodType { get; private set; }

        public Func<HttpContent> CreateContent { get; private set; }

        public Stream Stream { get; private set; }

        public object Data { get; private set; }

        public object Included { get; private set; }

        public bool NeedJsonApi { get; private init; } = false;

        public bool IsAuthorized { get; private init; } = true;

        public bool NeedDataKey { get; private init; } = true;

        public bool UseAppToken { get; private set; } = false;

        public string ContentType { get; private init; } = "application/vnd.api+json";

        public RangeHeaderValue RangeHeaderValue { get; private set; }

        public static ForgeSettings UnauthorizedPost(Func<HttpContent> createContent)
            => new ()
            {
                MethodType = HttpMethod.Post,
                CreateContent = createContent,
                IsAuthorized = false,
            };

        public static ForgeSettings AuthorizedGet()
            => new ()
            {
                MethodType = HttpMethod.Get,
            };

        public static ForgeSettings AuthorizedDelete()
            => new ()
            {
                MethodType = HttpMethod.Delete,
            };

        public static ForgeSettings AuthorizedPost(object data)
            => new ()
            {
                MethodType = HttpMethod.Post,
                Data = data,
            };

        public static ForgeSettings AuthorizedPostWithoutKeyData(object data)
            => new ()
            {
                MethodType = HttpMethod.Post,
                Data = data,
                NeedDataKey = false,
            };

        public static ForgeSettings AuthorizedPostWithJsonApi(object data)
            => new ()
            {
                MethodType = HttpMethod.Post,
                Data = data,
                NeedJsonApi = true,
            };

        public static ForgeSettings AuthorizedPostWithJsonApi(object data, object included)
            => new ()
            {
                MethodType = HttpMethod.Post,
                Data = data,
                Included = included,
                NeedJsonApi = true,
            };

        public static ForgeSettings AuthorizedPut(Stream stream)
            => new ()
            {
                MethodType = HttpMethod.Put,
                Stream = stream,
            };

        public static ForgeSettings AuthorizedPut(Stream stream, RangeHeaderValue range)
            => new ()
            {
                MethodType = HttpMethod.Put,
                Stream = stream,
                RangeHeaderValue = range,
            };

        public static ForgeSettings AuthorizedPatch(object data)
            => new ()
            {
                MethodType = HttpMethod.Patch,
                Data = data,
            };

        public static ForgeSettings AppGet()
            => new ()
            {
                MethodType = HttpMethod.Get,
                UseAppToken = true,
            };

        public static ForgeSettings AppPostWithoutKeyData(object data)
            => new ()
            {
                MethodType = HttpMethod.Post,
                Data = data,
                NeedDataKey = false,
                UseAppToken = true,
            };

        public static ForgeSettings AppPostWithoutKeyData(object data, string contentType)
            => new ()
            {
                MethodType = HttpMethod.Post,
                Data = data,
                NeedDataKey = false,
                UseAppToken = true,
                ContentType = contentType,
            };
    }
}
