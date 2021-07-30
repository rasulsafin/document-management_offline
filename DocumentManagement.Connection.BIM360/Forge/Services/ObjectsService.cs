using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
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
        /// <param name="objectName">URL-encoded object name being uploaded.</param>
        /// <param name="fileFullName">File path on this device.</param>
        /// <returns>Task of this action.</returns>
        public async Task<UploadResult> PutObjectAsync(string bucketKey, string objectName, string fileFullName)
        {
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPut(File.OpenRead(fileFullName)),
                    Resources.PutBucketsObjectsMethod,
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
            var tasks = new List<Task<JToken>>();

            for (long i = 0; i < length; i += chunkSize)
            {
                tasks.Add(connection.SendAsync(
                        ForgeSettings.AuthorizedPut(
                                File.OpenRead(file.FullName),
                                new RangeHeaderValue(i, Math.Max(i + chunkSize, length - 1))),
                        Resources.PutBucketsObjectsResumableMethod,
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

        public async Task<FileInfo> GetAsync(string bucketKey, string objectName, string fullFileName)
        {
            var directory = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            await using var output = File.OpenWrite(fullFileName);
            await using var stream = await connection.GetResponseStreamAuthorizedAsync(
                    HttpMethod.Get,
                    Resources.GetBucketsObjectMethod,
                    bucketKey,
                    objectName);
            await stream.CopyToAsync(output);
            return new FileInfo(fullFileName);
        }

        public async Task DeleteAsync(string bucketKey, string objectKey)
            => await connection.SendAsync(
                    ForgeSettings.AuthorizedDelete(),
                    Resources.DeleteBucketsObjectMethod,
                    bucketKey,
                    objectKey);

        public async Task<JToken> GetBuckets()
            => await connection.SendAsync(ForgeSettings.AuthorizedGet(), Resources.GetBucketsMethod);

        public async Task<JToken> GetBucketDetails(string bucketKey)
            => await connection.SendAsync(ForgeSettings.AuthorizedGet(), Resources.GetBucketDetailsMethod, bucketKey);

        public async Task<JToken> PostBucket(string bucketKeyToAdd)
        {
            var bucket = new
            {
                bucketKey = bucketKeyToAdd,
                access = "full",
                policyKey = "transient",
            };
            var response = await connection.SendAsync(
                    ForgeSettings.AuthorizedPostWithoutKeyData(bucket),
                    Resources.PostBucketsMethod);
            return response;
        }
    }
}
