using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    public class ProjectsService
    {
        private readonly ForgeConnection connection;

        public ProjectsService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<List<Project>> GetProjectsAsync(string hubId)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetProjectsOfHubMethod,
                    hubId);
            var data = response[DATA_PROPERTY]?.ToObject<List<Project>>();

            return data;
        }

        public async Task<Project> GetProjectAsync(string hubId, string projectId)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetProjectOfHubMethod,
                    hubId,
                    projectId);
            var data = response[DATA_PROPERTY]?.ToObject<Project>();

            return data;
        }

        public async Task<Hub> GetHubAsync(string hubId, string projectId)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetProjectsHubMethod,
                    hubId,
                    projectId);
            var data = response[DATA_PROPERTY]?.ToObject<Hub>();

            return data;
        }

        public async Task<List<Folder>> GetTopFoldersAsync(string hubId, string projectId)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetTopFoldersMethod,
                    hubId,
                    projectId);
            var data = response[DATA_PROPERTY]?.ToObject<List<Folder>>();

            return data;
        }

        public async Task<Download> GetDownloadInfoAsync(string projectId, string downloadId)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetProjectsDownloadInfoMethod,
                    projectId,
                    downloadId);
            var data = response[DATA_PROPERTY]?.ToObject<Download>();

            return data;
        }

        public async Task<StorageObject> CreateStorageAsync(string projectId, StorageObject obj)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPost(obj),
                    Resources.PostProjectStorageMethod,
                    projectId);

            return response[DATA_PROPERTY]?.ToObject<StorageObject>();
        }
    }
}
