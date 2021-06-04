using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro
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
