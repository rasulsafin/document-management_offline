using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Properties;

namespace Brio.Docs.Connections.Bim360.Forge.Services
{
    internal class BucketsService
    {
        private readonly ForgeConnection connection;

        public BucketsService(ForgeConnection connection)
            => this.connection = connection;

        public async Task<Bucket> PostBucketAsync(Bucket bucket)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AppPostWithoutKeyData(bucket, "application/json"),
                Resources.PostBucketsMethod);
            return response?.ToObject<Bucket>();
        }

        public async Task<List<Bucket>> GetBucketsAsync()
        {
            var response = await connection.SendAsync(ForgeSettings.AppGet(), Resources.GetBucketsMethod);
            return response[Constants.ITEMS_PROPERTY]?.ToObject<List<Bucket>>();
        }

        public async Task<Bucket> GetBucketDetailsAsync(string bucketKey)
        {
            var response = await connection.SendAsync(
                ForgeSettings.AppGet(),
                Resources.GetBucketDetailsMethod,
                bucketKey);
            return response?.ToObject<Bucket>();
        }
    }
}
