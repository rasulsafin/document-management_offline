using System.Runtime.Serialization;

namespace DocumentManagement.Connection.BIM360.Forge.Models.DataManagement
{
    public class Download : Object<Download.DownloadAttributes, Download.DownloadRelationships>
    {
        public class DownloadAttributes : AAttributes
        {
            [DataMember(Name = "format")]
            public dynamic Format { get; set; }
        }

        public class DownloadRelationships
        {
            [DataMember(Name = "storage")]
            public dynamic Storage { get; set; }
        }
    }
}
