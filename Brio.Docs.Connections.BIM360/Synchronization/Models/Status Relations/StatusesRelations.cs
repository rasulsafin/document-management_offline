using System.Runtime.Serialization;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;

namespace Brio.Docs.Connections.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    internal class StatusesRelations
    {
        [DataMember]
        public RelationRule<Status, ObjectiveStatus>[] Get { get; set; }

        [DataMember]
        public RelationRule<ObjectiveStatus, Status>[] Set { get; set; }
    }
}
