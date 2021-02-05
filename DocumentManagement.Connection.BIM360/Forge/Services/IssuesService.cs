using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Forge;
using DocumentManagement.Connection.BIM360.Properties;
using Forge.Models;

namespace Forge.Services
{
    public class IssuesService
    {
        private static readonly int ITEMS_ON_PAGE = 100;
        private static readonly string DATA_PROPERTY = "data";
        private static readonly string META_PROPERTY = "meta";
        private static readonly string RESULTS_PROPERTY = "results";

        private readonly Connection connection;

        public IssuesService(Connection connection)
        {
            this.connection = connection;
        }

        public async Task<List<Issue>> GetIssuesAsync(string containerID)
        {
            var result = new List<Issue>();
            var all = false;

            for (int i = 0; !all; i++)
            {
                var response = await connection
                        .GetResponse(HttpMethod.Get, Resources.GetIssuesMethod, containerID, ITEMS_ON_PAGE, i);
                var data = response[DATA_PROPERTY]?.ToObject<List<Issue>>();
                if (data != null)
                    result.AddRange(data);
                var meta = response[META_PROPERTY]?.ToObject<Meta>();
                all = meta == null || meta.Page.Limit * ((meta.Page.Offset / ITEMS_ON_PAGE) + 1) >= meta.RecordCount;
            }

            return result;
        }

        public async Task<Attachment> PostIssuesAttachmentsAsync(string containerID, Attachment attachment)
        {
            var response = await connection.SendRequestWithSerializedData(HttpMethod.Post,
                    Resources.PostIssuesAttachmentsMethod,
                    attachment,
                    containerID);
            return response[DATA_PROPERTY]?.ToObject<Attachment>();
        }

        public async Task<List<IssueType>> GetIssueTypesAsync(string containerID)
        {
            var response = await connection.GetResponse(HttpMethod.Get, Resources.GetNGIssueTypesMethod, containerID);
            return response[RESULTS_PROPERTY]?.ToObject<List<IssueType>>();
        }

        public async Task<Issue> PostIssueAsync(string containerID, Issue issue)
        {
            var response = await
                    connection.SendRequestWithSerializedData(HttpMethod.Post, Resources.PostIssuesMethod, issue, containerID);
            return response[DATA_PROPERTY]?.ToObject<Issue>();
        }

        public async Task<Issue> PatchIssueAsync(string containerID, Issue issue)
        {
            var response = await connection.SendRequestWithSerializedData(
                    HttpMethod.Patch,
                    Resources.PostIssuesMethod,
                    issue,
                    containerID,
                    issue.id);
            return response[DATA_PROPERTY]?.ToObject<Issue>();
        }
    }
}
