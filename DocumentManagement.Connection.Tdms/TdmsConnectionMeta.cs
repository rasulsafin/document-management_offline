using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Tdms
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
