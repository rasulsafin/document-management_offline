using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
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
                var response = await connection.SendAsync(
                        ForgeSettings.AuthorizedGet(),
                        Resources.GetIssuesMethod,
                        containerID,
                        ITEMS_ON_PAGE,
                        i);
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
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPost(attachment),
                    Resources.PostIssuesAttachmentsMethod,
                    containerID);
            return response[DATA_PROPERTY]?.ToObject<Attachment>();
        }

        public async Task<List<IssueType>> GetIssueTypesAsync(string containerID)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetNGIssueTypesMethod,
                    containerID);
            return response[RESULTS_PROPERTY]?.ToObject<List<IssueType>>();
        }

        public async Task<Issue> PostIssueAsync(string containerID, Issue issue)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPost(issue),
                    Resources.PostIssuesMethod,
                    containerID);
            return response[DATA_PROPERTY]?.ToObject<Issue>();
        }

        public async Task<Issue> PatchIssueAsync(string containerID, Issue issue)
        {
            try
            {
               // var content = new StringContent();
                var response = await connection.SendAsync(
                        ForgeSettings.AuthorizedPatch(issue),
                        Resources.PatchIssuesMethod,
                        containerID,
                        issue.ID);
                return response[DATA_PROPERTY]?.ToObject<Issue>();

            }
            finally
            {
            }
        }

        public async Task<Issue> GetIssueAsync(string containerID, string issueID)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedGet(),
                    Resources.GetIssueMethod,
                    containerID,
                    issueID);
            return response[DATA_PROPERTY]?.ToObject<Issue>();
        }
    }
}
