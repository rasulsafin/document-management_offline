using System;
using System.Runtime.Serialization;
using Brio.Docs.Connections.Bim360.Forge.Utils.Extensions.JsonConverters;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class Bucket
    {
        [DataMember(Name = "bucketKey")]
        public string BucketKey { get; set; }

        [DataMember(Name = "bucketOwner")]
        public object[] BucketOwner { get; set; }

        [DataMember(Name = "allow")]
        public object[] Allow { get; set; }

        [JsonConverter(typeof(MillisecondsUnixDateTimeConverter))]
        [DataMember(Name = "createdDate")]
        public DateTime? CreatedDate { get; set; }

        [DataMember(Name = "access")]
        public BucketAccess? Access { get; set; }

        [DataMember(Name = "permissions")]
        public object[] Permissions { get; set; }

        [DataMember(Name = "policyKey")]
        public OssRetentionPolicy? PolicyKey { get; set; }
    }
}
