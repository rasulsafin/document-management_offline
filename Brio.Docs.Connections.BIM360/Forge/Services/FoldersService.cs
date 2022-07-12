using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Forge.Utils.Pagination;
using Brio.Docs.Connections.Bim360.Properties;
using Newtonsoft.Json.Linq;
using static Brio.Docs.Connections.Bim360.Forge.Constants;
using Version = Brio.Docs.Connections.Bim360.Forge.Models.DataManagement.Version;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    public class FoldersService
    {
        private readonly ForgeConnection connection;

        public FoldersService(ForgeConnection connection)
            => this.connection = connection;

        public IAsyncEnumerable<Folder> GetFoldersAsync(
            string projectId,
            string folderId,
            IEnumerable<IQueryParameter> parameters = null)
        {
            parameters ??= Enumerable.Empty<Filter>();
            parameters = parameters.Append(new Filter(TYPE_PROPERTY, FOLDER_TYPE));
            var command = ForgeConnection.SetParameters(Resources.GetProjectsFoldersContentsMethod, parameters);
            return PaginationHelper.GetItemsByPages<Folder, LinksStrategy>(
                connection,
                command,
                DATA_PROPERTY,
                projectId,
                folderId);
        }

        public IAsyncEnumerable<(Item item, Version version)> GetItemsAsync(
            string projectId,
            string folderId,
            IEnumerable<IQueryParameter> parameters = null)
        {
            parameters ??= Enumerable.Empty<Filter>();
            parameters = parameters.Append(new Filter(TYPE_PROPERTY, ITEM_TYPE));
            var command = ForgeConnection.SetParameters(Resources.GetProjectsFoldersContentsMethod, parameters);
            return PaginationHelper.GetItemsByPages<(Item item, Version version), LinksStrategy>(
                connection,
                command,
                ConvertToItemsVersions,
                projectId,
                folderId);
        }

        public async Task<List<(Version version, Item item)>> SearchAsync(
            string projectId,
            string folderId,
            IEnumerable<Filter> filters = null)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                ForgeConnection.SetParameters(Resources.GetProjectsFoldersSearchMethod, filters),
                projectId,
                folderId);

            var versions = response[DATA_PROPERTY]?.ToObject<Version[]>();
            var items = response[INCLUDED_PROPERTY]?.ToObject<Item[]>() ?? Array.Empty<Item>();

            return versions?
               .Select(v => (v, items.FirstOrDefault(i => i.ID == v.Relationships.Item?.Data.ID)))
               .ToList();
        }

        private IEnumerable<(Item item, Version version)> ConvertToItemsVersions(JToken response)
        {
            var items = response[DATA_PROPERTY]?.ToObject<Item[]>();
            var versions = response[INCLUDED_PROPERTY]?.ToObject<Version[]>() ?? Array.Empty<Version>();
            var result = items?.Select(
                item => (item, versions.FirstOrDefault(vers => vers.Relationships.Item.Data.ID == item.ID)));
            return result ?? ArraySegment<(Item item, Version version)>.Empty;
        }
    }
}
