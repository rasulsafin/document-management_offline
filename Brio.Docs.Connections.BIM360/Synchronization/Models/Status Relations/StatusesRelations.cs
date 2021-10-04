using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    internal class StatusesRelations
    {
        [DataMember]
        public RelationRule<Status, ObjectiveStatus>[] Get { get; set; }

        [DataMember]
        public RelationRule<ObjectiveStatus, Status>[] Set { get; set; }

        [DataMember]
        public Status[] Priority { get; set; }
    }
}
