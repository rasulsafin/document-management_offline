using System;

namespace MRS.Bim.DocumentManagement.Utilities
{
    public class CloudItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public long? Size { get; set; }
    }
}
