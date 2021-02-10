using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.BIM360.Forge.Models;
using MRS.DocumentManagement.Connection.BIM360.Properties;
using static MRS.DocumentManagement.Connection.BIM360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Services
{
    public class IssuesService
    {
        private readonly ForgeConnection connection;

        public IssuesService(ForgeConnection connection)
            => this.connection = connection;

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
                    Resources.PatchIssuesMethod,
                    issue,
                    containerID,
                    issue.ID);
            return response[DATA_PROPERTY]?.ToObject<Issue>();
        }
    }
}
