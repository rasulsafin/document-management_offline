using System;
using System.Collections.Generic;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
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
