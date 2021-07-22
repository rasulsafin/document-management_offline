using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using static System.Net.Mime.MediaTypeNames;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class AttachmentsService : Service
    {
        private static readonly string BASE_URL = "/attachment";

        public AttachmentsService(MrsProHttpConnection connection)
            : base(connection)
        {
        }

        internal async Task<IEnumerable<Attachment>> GetAll(DateTime date)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var unixDate = date.ToUnixTime();
            var list = listOfAllObjectives.Where(o => o.CreatedDate > unixDate).ToArray();
            return list;
        }

        internal async Task<IEnumerable<Attachment>> GetByOwnerId(string id)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var list = listOfAllObjectives.Where(o => o.Owner == id).ToArray();
            return list;
        }

        internal async Task<IEnumerable<Attachment>> GetByParentId(string id)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var list = listOfAllObjectives.Where(o => o.ParentId == id).ToArray();
            return list;
        }

        internal async Task<Attachment> TryDownloadAttachmentById(string id)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var attachment = listOfAllObjectives.Where(o => o.Id == id).ToArray()[0];

            string link = "https://s3-eu-west-1.amazonaws.com/plotpad-org/" + attachment.UrlToFile;
            string dirPath = "Downloads\\";
            string path = dirPath + attachment.OriginalFileName;

            WebClient webClient = new WebClient();
            try
            {
                Directory.CreateDirectory(dirPath);
                await webClient.DownloadFileTaskAsync(new Uri(link), path);
                return attachment;
            }
            catch
            {
                return null;
            }
        }

        internal async Task<bool> TryUploadAttachment(PhotoAttachmentData attachment,
                                                string id,
                                                string originalName,
                                                string parentId,
                                                string parentType,
                                                byte[] file)
        {
            string query = $"?id={id}&originalName={originalName}&parentId={parentId}&parentType={parentType}";
            try
            {
                await HttpConnection.PutJson<PhotoAttachmentData>(BASE_URL + query, attachment, file);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal async Task<IEnumerable<Attachment>> TryGetByIds(IReadOnlyCollection<string> ids)
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

        internal async Task<bool> TryDeleteById(string id)
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

        internal async Task<Attachment> TryPost(Attachment attachment)
        {
            try
            {
                var result = await HttpConnection.PostJson<Attachment>(BASE_URL, attachment);
                   // new Attachment() { OriginalName = "image.png", ParentId = "60ed826800fac340ae7049fe", ParentType = "task" }
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
