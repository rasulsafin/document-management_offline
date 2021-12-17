using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Forge.Utils.Pagination;
using Brio.Docs.Connections.Bim360.Properties;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    public class FoldersService
    {
        private readonly ForgeConnection connection;

        public FoldersService(ForgeConnection connection)
            => this.connection = connection;

        public IAsyncEnumerable<Folder> GetFoldersAsync(string projectId, string folderId)
            => PaginationHelper.GetItemsByPages<Folder, LinksStrategy>(
                connection,
                ForgeConnection.SetParameter(
                    Resources.GetProjectsFoldersContentsMethod,
                    new Filter(TYPE_PROPERTY, FOLDER_TYPE)),
                DATA_PROPERTY,
                projectId,
                folderId);

        public IAsyncEnumerable<(Item item, Models.DataManagement.Version version)> GetItemsAsync(
            string projectId,
            string folderId)
            => PaginationHelper.GetItemsByPages<(Item item, Models.DataManagement.Version version), LinksStrategy>(
                connection,
                ForgeConnection.SetParameter(
                    Resources.GetProjectsFoldersContentsMethod,
                    new Filter(TYPE_PROPERTY, ITEM_TYPE)),
                response =>
                {
                    var items = response[DATA_PROPERTY]?.ToObject<Item[]>();
                    var versions = response[INCLUDED_PROPERTY]?.ToObject<Models.DataManagement.Version[]>() ??
                        Array.Empty<Models.DataManagement.Version>();
                    return items?.Select(
                            item => (item,
                                versions.FirstOrDefault(vers => vers.Relationships.Item.Data.ID == item.ID))) ??
                        ArraySegment<(Item item, Models.DataManagement.Version version)>.Empty;
                },
                projectId,
                folderId);

        public async Task<List<(Models.DataManagement.Version version, Item item)>> SearchAsync(
                string projectId,
                string folderId,
                IEnumerable<Filter> filters = null)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                ForgeConnection.SetParameters(Resources.GetProjectsFoldersSearchMethod, filters),
                projectId,
                folderId);

            var versions = response[DATA_PROPERTY]?.ToObject<Models.DataManagement.Version[]>();
            var items = response[INCLUDED_PROPERTY]?.ToObject<Item[]>() ?? Array.Empty<Item>();

            return versions?
                    .Select(v => (v, items.FirstOrDefault(i => i.ID == v.Relationships.Item?.Data.ID)))
                    .ToList();
        }
    }
}
