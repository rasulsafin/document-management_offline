using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;

namespace Brio.Docs.Connections.Tdms
{
    public class TdmsConnectionMeta : IConnectionMeta
    {
        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = "tdms",
                AuthFieldNames = new List<string>
                {
                    Auth.LOGIN,
                    Auth.PASSWORD,
                    Auth.SERVER,
                    Auth.DATABASE,
                },
                AppProperties = new Dictionary<string, string>(),
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(TdmsConnection);
    }
}
