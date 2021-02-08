using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge;
using DocumentManagement.Connection.BIM360.Properties;
using Forge.Models.DataManagement;
using static Forge.Constants;
using Version = Forge.Models.DataManagement.Version;

namespace Forge.Services
{
    public class FoldersService
    {
        private readonly Connection connection;

        public FoldersService(Connection connection)
            => this.connection = connection;

        public async Task<List<Folder>> GetFoldersAsync(string projectId, string folderId)
        {
            var response = await connection.GetResponse(HttpMethod.Get,
                    Resources.GetProjectsFoldersContentsMethod,
                    projectId,
                    folderId,
                    FOLDER_TYPE);

            return response[DATA_PROPERTY]?.ToObject<List<Folder>>();
        }

        public async Task<List<Item>> GetItemsAsync(string projectId, string folderId)
        {
            var response = await connection.GetResponse(HttpMethod.Get,
                    Resources.GetProjectsFoldersContentsMethod,
                    projectId,
                    folderId,
                    ITEM_TYPE);

            return response[DATA_PROPERTY]?.ToObject<List<Item>>();
        }

        public async Task<List<(Version version, Item item)>> SearchAsync(string projectId, string folderId, (string filteringField, string filteringValue)[] filters)
        {
            var stringBuilder = new StringBuilder(Resources.GetProjectsFoldersSearchMethod);
            foreach (var filter in filters)
                stringBuilder.AppendFormat("filter[{0}]={1}&", filter.filteringField, filter.filteringValue);
            var response = await connection.GetResponse(HttpMethod.Get,
                    stringBuilder.ToString(),
                    projectId,
                    folderId);

            var versions = response[DATA_PROPERTY]?.ToObject<Version[]>();
            var items = response[INCLUDED_PROPERTY]?.ToObject<Item[]>() ?? Array.Empty<Item>();

            return versions?.Select(v => (v, items.FirstOrDefault(i => i.ID == v.Relationships.Item.data.id))).ToList();
        }
    }
}
