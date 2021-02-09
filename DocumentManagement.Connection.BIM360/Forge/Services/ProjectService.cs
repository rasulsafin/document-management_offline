using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge;
using DocumentManagement.Connection.BIM360.Properties;
using Forge.Models.DataManagement;
using static Forge.Constants;

namespace Forge.Services
{
    public class ProjectService
    {
        private readonly Connection connection;

        public ProjectService(Connection connection)
            => this.connection = connection;

        public async Task<List<Project>> GetProjectsAsync(string hubId)
        {
            var response = await connection
                .GetResponse(HttpMethod.Get, Resources.GetProjectsOfHubMethod, hubId);
            var data = response[DATA_PROPERTY].ToObject<List<Project>>();

            return data;
        }

        public async Task<Project> GetProjectAsync(string hubId, string projectId)
        {
            var response = await connection
                .GetResponse(HttpMethod.Get, Resources.GetProjectOfHubMethod, hubId, projectId);
            var data = response[DATA_PROPERTY].ToObject<Project>();

            return data;
        }

        public async Task<Hub> GetHubAsync(string hubId, string projectId)
        {
            var response = await connection
                .GetResponse(HttpMethod.Get, Resources.GetProjectsHubMethod, hubId, projectId);
            var data = response[DATA_PROPERTY].ToObject<Hub>();

            return data;
        }

        public async Task<List<Folder>> GetTopFoldersAsync(string hubId, string projectId)
        {
            var response = await connection
                .GetResponse(HttpMethod.Get, Resources.GetTopFoldersMethod, hubId, projectId);
            var data = response[DATA_PROPERTY].ToObject<List<Folder>>();

            return data;
        }

        public async Task<Download> GetDownloadInfoAsync(string projectId, string downloadId)
        {
            var response = await connection
                .GetResponse(HttpMethod.Get, Resources.GetProjectsDownloadInfoMethod, projectId, downloadId);
            var data = response[DATA_PROPERTY].ToObject<Download>();

            return data;
        }

        public async Task<string> CreateStorageAsync(string projectId, string fileName, string folderId, bool isFolder)
        {
            var obj = new StorageObject
            {
                Type = "objects",
                Attributes = new StorageObject.StorageObjectAttributes { Name = fileName },
                Relationships = new StorageObject.StorageObjectRelationshops
                {
                    Target = new
                    {
                        data = new
                        {
                            type = isFolder ? FOLDER_TYPE : ITEM_TYPE,
                            id = folderId,
                        },
                    },
                },
            };
            var response = await connection
                .SendRequestWithSerializedData(HttpMethod.Post, Resources.PostProjectStorageMethod, obj, projectId);

            return response[DATA_PROPERTY]?.ToObject<StorageObject>().ID;
        }
    }
}
