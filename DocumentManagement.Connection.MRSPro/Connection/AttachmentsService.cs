using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class AttachmentsService : Service
    {
        private static readonly string BASE_URL = "/attachment";
        private static readonly string SERVER_URL = "https://s3-eu-west-1.amazonaws.com/plotpad-org/";

        public AttachmentsService(MrsProHttpConnection connection)
            : base(connection)
        {
        }

        internal async Task<IEnumerable<Attachment>> GetAllAsync(DateTime date)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var unixDate = date.ToUnixTime();
            var list = listOfAllObjectives.Where(o => o.CreatedDate > unixDate).ToArray();
            return list;
        }

        internal async Task<Attachment> GetByIdAsync(string id)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var attachment = listOfAllObjectives.Where(o => o.Id == id).ToArray()[0];

            return attachment;
        }

        internal async Task<IEnumerable<Attachment>> GetByOwnerIdAsync(string id)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var list = listOfAllObjectives.Where(o => o.Owner == id).ToArray();
            return list;
        }

        internal async Task<IEnumerable<Attachment>> GetByParentIdAsync(string id)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var list = listOfAllObjectives.Where(o => o.ParentId == id).ToArray();
            return list;
        }

        internal async Task<Uri> GetAttachmentUriAsync(string id)
        {
            var attachment = await GetByIdAsync(id);

            string link = SERVER_URL + attachment.UrlToFile;

            var uri = new Uri(link);

            return uri;
        }

        internal async Task<bool> TryUploadAttachmentAsync(PhotoAttachmentData attachment,
                                                string id,
                                                string originalName,
                                                string parentId,
                                                string parentType,
                                                byte[] file)
        {
            string query = $"?id={id}&originalName={originalName}&parentId={parentId}&parentType={parentType}";
            try
            {
                await HttpConnection.PutJson<PhotoAttachmentData>(BASE_URL + query, attachment, file, originalName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal async Task<IEnumerable<Attachment>> TryGetByIdsAsync(IReadOnlyCollection<string> ids)
        {
            try
            {
                var idsStr = string.Join(QUERY_SEPARATOR, ids);
                return await HttpConnection.GetListOf<Attachment>(GetByIds(BASE_URL), idsStr);
            }
            catch
            {
                return null;
            }
        }

        internal async Task<bool> TryDeleteByIdAsync(string id)
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

        internal async Task<IEnumerable<Attachment>> TryGetByParentIdAsync(string parentId)
        {
            try
            {
                var res = await HttpConnection.GetListOf<Attachment>(GetByParentPath(BASE_URL), parentId);
                return res;
            }
            catch
            {
                return null;
            }
        }
    }
}
