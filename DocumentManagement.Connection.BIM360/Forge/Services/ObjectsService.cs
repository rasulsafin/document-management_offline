using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DocumentManagement.Connection.BIM360.Properties;
using Newtonsoft.Json.Linq;

namespace DocumentManagement.Connection.BIM360.Forge.Services
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
        public async Task PutObjectAsync(string bucketKey, FileInfo file)
            => await connection.SendRequestWithStream(HttpMethod.Patch,
                    Resources.PutBucketsObjectsMethod,
                    System.IO.File.OpenRead(file.FullName),
                    null,
                    bucketKey,
                    file.Name);

        /// <summary>
        /// Recommend to objects larger than 100 MB.
        /// </summary>
        /// <param name="bucketKey">URL-encoded bucket to upload object into.</param>
        /// <param name="file">URL-encoded object name being uploaded.</param>
        /// <returns>Task of this action.</returns>
        public async Task PutObjectResumableAsync(string bucketKey, FileInfo file, long chunkSize)
        {
            // TODO: check is it working or not?
            var length = file.Length;
            var tasks = new List<Task<JObject>>();

            for (long i = 0; i < length; i += chunkSize)
            {
                tasks.Add(connection.SendRequestWithStream(HttpMethod.Patch,
                        Resources.PutBucketsObjectsResumableMethod,
                        System.IO.File.OpenRead(file.FullName),
                        new RangeHeaderValue(i, Math.Max(i + chunkSize, length - 1)),
                        bucketKey,
                        file.Name));
            }

            foreach (var task in tasks)
                await task;
        }
    }
}
