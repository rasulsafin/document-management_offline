using System;
using System.Collections.Generic;
using Brio.Docs.Integration.Client;
using Brio.Docs.Integration.Dtos;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro
{
    public class MrsProConnectionMeta : IConnectionMeta
    {
        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = "MRSPro",
                AuthFieldNames = new List<string>
                {
                    COMPANY_CODE,
                    AUTH_EMAIL,
                    AUTH_PASS,
                },
                AppProperties = new Dictionary<string, string> { },
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(MrsProConnection);
    }
}
