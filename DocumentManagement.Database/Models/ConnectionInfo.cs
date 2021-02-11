using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class ConnectionInfo
    {
        public int ID { get; set; }

        public int ConnectionTypeID { get; set; }

        public ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// TODO: Security.
        /// </summary>
        public string AuthFieldValues { get; set; }

        public User User { get; set; }

        public ICollection<ConnectionInfoEnumerationType> EnumerationTypes { get; set; }

        public ICollection<ConnectionInfoEnumerationValue> EnumerationValues { get; set; }
    }
}
