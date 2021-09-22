using System.Runtime.Serialization;

namespace Brio.Docs.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public class Download : Object<Download.DownloadAttributes, Download.DownloadRelationships>
    {
        [DataContract]
        public class DownloadAttributes : AAttributes
        {
            [DataMember(Name = "format")]
            public dynamic Format { get; set; }
        }

        [DataContract]
        public class DownloadRelationships
        {
            [DataMember(Name = "storage")]
            public dynamic Storage { get; set; }
        }
    }
}
