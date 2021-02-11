using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Data_Management;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Services
{
    public class ObjectsService
    {
        private readonly ForgeConnection connection;

        public ObjectsService(ForgeConnection connection)
            => this.connection = connection;

        /// <summary>
        /// Recommend to objects smaller than 100 MB.
        /// </summary>
        /// <param name="bucketKey">URL-encoded bucket to upload object into.</param>
        /// <param name="file">URL-encoded object name being uploaded.</param>
        /// <returns>Task of this action.</returns>
        public async Task<UploadResult> PutObjectAsync(string bucketKey, string objectName, string fileFullName)
        {
            var response = await connection.SendStreamAuthorizedAsync(HttpMethod.Patch,
                    Resources.PutBucketsObjectsMethod,
                    File.OpenRead(fileFullName),
                    null,
                    bucketKey,
                    objectName);
            return response.ToObject<UploadResult>();
        }

        /// <summary>
        /// Recommend to objects larger than 100 MB.
        /// </summary>
        /// <param name="bucketKey">URL-encoded bucket to upload object into.</param>
        /// <param name="file">URL-encoded object name being uploaded.</param>
        /// <param name="chunkSize">Count of bytes, that must be upload by one request.</param>
        /// <returns>Task of this action.</returns>
        public async Task<UploadResult> PutObjectResumableAsync(string bucketKey, string objectName, FileInfo file, long chunkSize)
        {
            // TODO: check is it working or not?
            var length = file.Length;
            var tasks = new List<Task<JObject>>();

            for (long i = 0; i < length; i += chunkSize)
            {
                tasks.Add(connection.SendStreamAuthorizedAsync(HttpMethod.Patch,
                        Resources.PutBucketsObjectsResumableMethod,
                        File.OpenRead(file.FullName),
                        new RangeHeaderValue(i, Math.Max(i + chunkSize, length - 1)),
                        bucketKey,
                        objectName));
            }

            UploadResult result = null;

            foreach (var task in tasks)
            {
                var response = await task;
                if (response.HasValues)
                    result = response.ToObject<UploadResult>();
            }

            return result;
        }

        public async Task<FileInfo> GetAsync(string bucketKey, string objectName, string pathToDownload)
        {
            var directory = Path.GetDirectoryName(pathToDownload);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            await using var output = File.OpenWrite(pathToDownload);
            await using var stream = await connection.GetResponseStreamAuthorizedAsync(
                    HttpMethod.Get,
                    Resources.GetBucketsObjectMethod,
                    bucketKey,
                    objectName);
            await stream.CopyToAsync(output);
            return new FileInfo(pathToDownload);
        }

        public async Task<JObject> GetBuckets()
        {
            var response = await connection.GetResponseAuthorizedAsync(HttpMethod.Get, Resources.GetBucketsMethod);
            return response;
        }

        public async Task<JObject> GetBucketDetails(string bucketKey)
        {
            var response = await connection.GetResponseAuthorizedAsync(HttpMethod.Get, Resources.GetBucketDetailsMethod, bucketKey);
            return response;
        }
    }
}
