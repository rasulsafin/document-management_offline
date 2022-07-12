using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Forge.Utils.Pagination;
using Brio.Docs.Connections.Bim360.Properties;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    public class IssuesService : IIssuesService
    {
        private readonly ForgeConnection connection;

        public IssuesService(ForgeConnection connection)
            => this.connection = connection;

        public IAsyncEnumerable<Issue> GetIssuesAsync(
            string containerID,
            IEnumerable<IQueryParameter> filters = null)
            => PaginationHelper.GetItemsByPages<Issue, MetaStrategy>(
                connection,
                ForgeConnection.SetParameters(Resources.GetIssuesMethod, filters),
                DATA_PROPERTY,
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

        public IAsyncEnumerable<Attachment> GetAttachmentsAsync(
            string containerID,
            string issueID,
            IEnumerable<IQueryParameter> parameters = null)
            => PaginationHelper.GetItemsByPages<Attachment, MetaStrategy>(
                connection,
                ForgeConnection.SetParameters(Resources.GetIssuesAttachmentMethod, parameters),
                DATA_PROPERTY,
                containerID,
                issueID);

        public async Task<List<RootCause>> GetRootCausesAsync(string containerID)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                Resources.GetRootCausesMethod,
                containerID);
            return response[DATA_PROPERTY]?.ToObject<List<RootCause>>();
        }

        public async Task<UserInfo> GetMeAsync(string containerID)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AuthorizedGet(),
                Resources.GetUsersMeMethod,
                containerID);
            return response[DATA_PROPERTY]?.ToObject<UserInfo>();
        }

        public IAsyncEnumerable<Comment> GetCommentsAsync(
            string containerID,
            string issueID,
            IEnumerable<IQueryParameter> parameters = null)
            => PaginationHelper.GetItemsByPages<Comment, MetaStrategy>(
                connection,
                ForgeConnection.SetParameters(Resources.GetIssuesCommentsMethod, parameters),
                DATA_PROPERTY,
                containerID,
                issueID);

        public async Task<Comment> PostIssuesCommentsAsync(string containerID, Comment comment)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPost(comment),
                    Resources.PostIssuesCommentsMethod,
                    containerID);
            return response[DATA_PROPERTY]?.ToObject<Comment>();
        }
    }
}