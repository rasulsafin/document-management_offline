using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;

namespace Brio.Docs.Connection.YandexDisk
{
    public class YandexConnectionMeta : IConnectionMeta
    {
        private const string NAME_CONNECTION = "Yandex Disk";

        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = NAME_CONNECTION,
                AuthFieldNames = new List<string>() { "token" },
                AppProperties = new Dictionary<string, string>
                {
                    { YandexDiskAuth.KEY_CLIENT_ID, "b1a5acbc911b4b31bc68673169f57051" },
                    { YandexDiskAuth.KEY_CLIENT_SECRET, "b4890ed3aa4e4a4e9e207467cd4a0f2c" },
                    { YandexDiskAuth.KEY_RETURN_URL, @"http://localhost:8000/oauth/" },
                },
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(YandexConnection);
    }
}
