using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class ConnectionType
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string AppProperty { get; set; }

        public string AuthFieldNames { get; set; }

        public ICollection<ConnectionInfo> ConnectionInfos { get; set; }

        public ICollection<ObjectiveType> ObjectiveTypes { get; set; }
    }
}
