using System.Collections.Generic;

namespace MRS.DocumentManagement.Database.Models
{
    public class ConnectionInfo
    {
        public int ID { get; set; }

        public int ConnectionTypeID { get; set; }

        public ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// TODO: Security??
        /// </summary>
        public ICollection<string> AuthFieldValues { get; set; }

        public User User { get; set; }

        /// <summary>
        /// TODO: Enums.
        /// </summary>
        public ICollection<EnumDm> EnumDms { get; set; }

        /// <summary>
        /// TODO: ObjectiveTypes.
        /// </summary>
        // public ICollection<ObjectiveType> ObjectiveTypes { get; set; }
    }
}
