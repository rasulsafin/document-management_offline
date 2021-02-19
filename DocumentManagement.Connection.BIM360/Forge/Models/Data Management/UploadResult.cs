using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class UploadResult
    {
        [DataMember(Name = "bucketKey")]
        public string BucketKey { get; set; }

        [DataMember(Name = "objectId")]
        public string ObjectId { get; set; }

        [DataMember(Name = "objectKey")]
        public string ObjectKey { get; set; }

        [DataMember(Name = "size")]
        public int? Size { get; set; }

        [DataMember(Name = "contentType")]
        public string ContentType { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }
    }
}
