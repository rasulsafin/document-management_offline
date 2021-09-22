using System;
using System.Collections.Generic;
using Brio.Docs.Integration.Client;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.GoogleDrive
{
    public class GoogleConnectionMeta : IConnectionMeta
    {
        private const string NAME_CONNECT = "Google Drive";

        public ConnectionTypeExternalDto GetConnectionTypeInfo()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = NAME_CONNECT,
                AuthFieldNames = new List<string>
                {
                    // Token stored as 'user' by sdk. See DataStore.StoreAsync
                    GoogleDriveController.USER_AUTH_FIELD_NAME,
                },
                AppProperties = new Dictionary<string, string>
                {
                    { GoogleDriveController.APPLICATION_NAME, "BRIO MRS" },
                    {
                        GoogleDriveController.CLIENT_ID,
                        "1827523568-ha5m7ddtvckjqfrmvkpbhdsl478rdkfm.apps.googleusercontent.com"
                    },
                    { GoogleDriveController.CLIENT_SECRET, "fA-2MtecetmXLuGKXROXrCzt" },
                },
            };

            return type;
        }

        public Type GetIConnectionType()
            => typeof(GoogleConnection);
    }
}
