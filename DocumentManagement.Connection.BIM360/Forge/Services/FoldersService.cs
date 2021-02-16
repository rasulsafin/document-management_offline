using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class FoldersService
    {
        private readonly ForgeConnection connection;

        public FoldersService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<List<Folder>> GetFoldersAsync(string projectId, string folderId)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetProjectsFoldersContentsMethod,
                    projectId,
                    folderId,
                    FOLDER_TYPE);

            return response[DATA_PROPERTY]?.ToObject<List<Folder>>();
        }

        public async Task<List<Item>> GetItemsAsync(string projectId, string folderId)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetProjectsFoldersContentsMethod,
                    projectId,
                    folderId,
                    ITEM_TYPE);

            return response[DATA_PROPERTY]?.ToObject<List<Item>>();
        }

        public async Task<List<(Version version, Item item)>> SearchAsync(
                string projectId,
                string folderId,
                IEnumerable<(string filteringField, string filteringValue)> filters)
        {
            var stringBuilder = new StringBuilder(Resources.GetProjectsFoldersSearchMethod);
            foreach ((string field, string filterValue) in filters)
                stringBuilder.AppendFormat(FILTER_QUERY_PARAMETER, field, filterValue);
            if (stringBuilder.Length > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    stringBuilder.ToString(),
                    projectId,
                    folderId);

            var versions = response[DATA_PROPERTY]?.ToObject<Version[]>();
            var items = response[INCLUDED_PROPERTY]?.ToObject<Item[]>() ?? Array.Empty<Item>();

            return versions?
                    .Select(v => (v, items.FirstOrDefault(i => i.ID == v.Relationships.Item?.Data.ID)))
                    .ToList();
        }
    }
}
