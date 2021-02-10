using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.BIM360.Properties;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Connection.BIM360.Forge.Services
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
        public async Task PutObjectAsync(string bucketKey, string objectName, string fileFullName)
            => await connection.SendStreamAuthorizedAsync(HttpMethod.Patch,
                    Resources.PutBucketsObjectsMethod,
                    File.OpenRead(fileFullName),
                    null,
                    bucketKey,
                    objectName);

        /// <summary>
        /// Recommend to objects larger than 100 MB.
        /// </summary>
        /// <param name="bucketKey">URL-encoded bucket to upload object into.</param>
        /// <param name="file">URL-encoded object name being uploaded.</param>
        /// <returns>Task of this action.</returns>
        public async Task PutObjectResumableAsync(string bucketKey, string objectName, FileInfo file, long chunkSize)
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

            foreach (var task in tasks)
                await task;
        }
    }
}
