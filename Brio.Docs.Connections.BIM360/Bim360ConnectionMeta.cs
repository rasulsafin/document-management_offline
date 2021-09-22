using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360
{
    public class Bim360ConnectionMeta : IConnectionMeta
    {
        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = "BIM360",
                AuthFieldNames = new List<string>
                {
                    TOKEN_AUTH_NAME,
                    REFRESH_TOKEN_AUTH_NAME,
                },
                AppProperties = new Dictionary<string, string>
                {
                    { CLIENT_ID_NAME, "m5fLEAiDRlW3G7vgnkGGGcg4AABM7hCf" },
                    { CLIENT_SECRET_NAME, "dEGEHfbl9LWmEnd7" },
                    { CALLBACK_URL_NAME, "http://localhost:8000/oauth/" },
                },
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(Bim360Connection);
    }
}
