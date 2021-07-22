﻿using System;
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

        internal async Task<IEnumerable<Attachment>> GetAllAsync(DateTime date)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var unixDate = date.ToUnixTime();
            var list = listOfAllObjectives.Where(o => o.CreatedDate > unixDate).ToArray();
            return list;
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

        internal async Task<Attachment> TryDownloadAttachmentByIdAsync(string id)
        {
            var listOfAllObjectives = await HttpConnection.GetListOf<Attachment>(BASE_URL);
            var attachment = listOfAllObjectives.Where(o => o.Id == id).ToArray()[0];

            string link = "https://s3-eu-west-1.amazonaws.com/plotpad-org/" + attachment.UrlToFile;
            string dirPath = "Downloads\\";
            string path = dirPath + attachment.OriginalName;

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
