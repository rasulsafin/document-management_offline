using System;
using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    internal class RelationRule<TSource, TDestination>
        where TSource : Enum
        where TDestination : Enum
    {
        [DataMember]
        public TSource Source { get; set; }

        [DataMember]
        public TDestination Destination { get; set; }

        [DataMember]
        public RelationCondition Condition { get; set; }
    }
}
