using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Interfaces;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class IssuesService : Service
    {
        private static readonly string BASE_URL = "/task";
        private static readonly string BASE_EXTRA_URL = $"/extra{BASE_URL}";
        private readonly AttachmentsService attachmentsService;

        public IssuesService(MrsProHttpConnection connection, AttachmentsService attachmentsService)
            : base(connection)
        {
            this.attachmentsService = attachmentsService;
        }

        internal async Task<IEnumerable<Issue>> GetAll(DateTime date)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Issue>(BASE_URL);
            var unixDate = date.ToUnixTime();
            var list = listOfAllObjectives.Where(o => o.LastModifiedDate > unixDate).ToArray();
            return list;
        }

        internal async Task<IEnumerable<Issue>> TryGetByIds(IReadOnlyCollection<string> ids)
        {
            try
            {
                var idsStr = string.Join(QUERY_SEPARATOR, ids);
                return await HttpConnection.GetListOf<Issue>(GetByIds(BASE_URL), idsStr);
            }
            catch
            {
                return null;
            }
        }

        internal async Task<Issue> TryGetById(string id)
        {
            try
            {
                var res = await HttpConnection.GetListOf<Issue>(GetByIds(BASE_URL), new[] { id });
                return res.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        internal async Task<Issue> TryPost(Issue element)
        {
            try
            {
                var result = await HttpConnection.PostJson<Issue>(BASE_URL, element);
                return result;
            }
            catch
            {
                return null;
            }
        }

        internal async Task<Issue> TryPatch(UpdatedValues valuesToPatch)
        {
            try
            {
                var result = await HttpConnection.PatchJson<IEnumerable<Issue>, UpdatedValues>(BASE_URL, valuesToPatch);
                return result.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        internal async Task<bool> TryDelete(string id)
        {
            try
            {
                await HttpConnection.DeleteJson(BASE_URL, new[] { id });
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal async Task<IEnumerable<IElementAttachment>> GetAttachments(string ancestry)
        {
            var result = await attachmentsService.TryGetByParentIdAsync(ancestry);
            return result;
        }

        internal async Task<IEnumerable<IssueExtraInfo>> TryGetAttachmentInfoByIds(IReadOnlyCollection<string> ids)
        {
            try
            {
                var idsStr = string.Join(QUERY_SEPARATOR, ids);
                return await HttpConnection.GetListOf<IssueExtraInfo>(GetByIds(BASE_EXTRA_URL), idsStr);
            }
            catch
            {
                return null;
            }
        }
    }
}
