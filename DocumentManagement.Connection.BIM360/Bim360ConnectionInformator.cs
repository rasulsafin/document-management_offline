using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360
{
    public class Bim360ConnectionInformator : IConnectionInfo
    {
        public ConnectionTypeExternalDto GetConnectionType()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = "BIM360",
                AuthFieldNames = new List<string>
                {
                    "token",
                    "refreshtoken",
                    "end",
                },
                AppProperties = new Dictionary<string, string>
                {
                    { "CLIENT_ID", "m5fLEAiDRlW3G7vgnkGGGcg4AABM7hCf" },
                    { "CLIENT_SECRET", "dEGEHfbl9LWmEnd7" },
                    { "RETURN_URL", "http://localhost:8000/oauth/" },
                },
            };

            return type;
        }

        public Type GetTypeOfConnection()
            => typeof(Bim360Connection);
    }
}
