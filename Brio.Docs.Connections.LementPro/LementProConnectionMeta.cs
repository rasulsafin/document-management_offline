using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using static Brio.Docs.Connections.LementPro.LementProConstants;

namespace Brio.Docs.Connections.LementPro
{
    public class LementProConnectionMeta : IConnectionMeta
    {
        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = "LementPro",
                AuthFieldNames = new List<string>
                {
                    AUTH_NAME_LOGIN,
                    AUTH_NAME_PASSWORD,
                    AUTH_NAME_TOKEN,
                    AUTH_NAME_END,
                },
                AppProperties = new Dictionary<string, string>(),
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(LementProConnection);
    }
}
