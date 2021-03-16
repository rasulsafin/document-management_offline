using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class IssuesService
    {
        private readonly ForgeConnection connection;

        public IssuesService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<List<Issue>> GetIssuesAsync(
            string containerID,
            IEnumerable<(string filteringField, string filteringValue)> filters = null)
            => await GetItemsByPages<Issue>(
                ForgeConnection.SetFilters(Resources.GetIssuesMethod, filters),
                containerID);

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
            // TODO: check required fields from user.
            if (string.IsNullOrEmpty(issue.Attributes.Title))
                throw new ArgumentException($"{nameof(Issue.IssueAttributes.Title)} is empty");
            if (string.IsNullOrEmpty(issue.Attributes.NgIssueTypeID))
                throw new ArgumentException($"{nameof(Issue.IssueAttributes.NgIssueTypeID)} is empty");
            if (string.IsNullOrEmpty(issue.Attributes.NgIssueSubtypeID))
                throw new ArgumentException($"{nameof(Issue.IssueAttributes.NgIssueSubtypeID)} is empty");

            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPost(issue),
                    Resources.PostIssuesMethod,
                    containerID);
            return response[DATA_PROPERTY]?.ToObject<Issue>();
        }

        public async Task<Issue> PatchIssueAsync(string containerID, Issue issue)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPatch(issue.GetPatchableIssue()),
                    Resources.PatchIssuesMethod,
                    containerID,
                    issue.ID);
            return response[DATA_PROPERTY]?.ToObject<Issue>();
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

        public async Task<List<Attachment>> GetAttachmentsAsync(string containerID, string issueID)
            => await GetItemsByPages<Attachment>(Resources.GetIssuesAttachmentMethod, containerID, issueID);

        public Task<object> GetMeAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<List<T>> GetItemsByPages<T>(string command, params object[] arguments)
        {
            var result = new List<T>();
            var all = false;
            var length = arguments.Length;
            Array.Resize(ref arguments, length + 2);
            arguments[length++] = ITEMS_ON_PAGE;

            for (int i = 0; !all; i++)
            {
                arguments[length] = i;
                var response = await connection.SendAsync(
                        ForgeSettings.AuthorizedGet(),
                        command,
                        arguments);
                var data = response[DATA_PROPERTY]?.ToObject<List<T>>();
                if (data != null)
                    result.AddRange(data);
                var meta = response[META_PROPERTY]?.ToObject<Meta>();
                all = meta == null || meta.Page.Limit * ((meta.Page.Offset / ITEMS_ON_PAGE) + 1) >= meta.RecordCount;
            }

            return result;
        }
    }
}
