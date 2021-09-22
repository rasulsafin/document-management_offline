using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using static Brio.Docs.Connection.MrsPro.Constants;

namespace Brio.Docs.Connection.MrsPro
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
