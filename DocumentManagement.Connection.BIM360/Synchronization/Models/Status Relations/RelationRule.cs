using System;
using System.Runtime.Serialization;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    internal class RelationRule<TSource, TDestination>
        where TSource : Enum
        where TDestination : Enum
    {
        [DataMember]
        [JsonConverter(typeof(SafeStringEnumConverter), 0)]
        public TSource Source { get; set; }

        [DataMember]
        [JsonConverter(typeof(SafeStringEnumConverter), 0)]
        public TDestination Destination { get; set; }

        [DataMember]
        public RelationCondition[] Conditions { get; set; }
    }
}
