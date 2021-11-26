using System;
using System.Runtime.Serialization;
using Brio.Docs.Connections.Bim360.Forge.Utils.JsonConverters;
using Brio.Docs.Connections.Bim360.Synchronization.Interfaces.StatusRelations;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    internal class RelationRule<TSource, TDestination> : IRelationRule
        where TSource : Enum
        where TDestination : Enum
    {
        [DataMember]
        [JsonConverter(typeof(SafeStringEnumConverter), 0)]
        public TSource Source { get; set; }

        [DataMember]
        [JsonConverter(typeof(SafeStringEnumConverter), 0)]
        public TDestination Destination { get; set; }

        Enum IRelationRule.Source => Source;

        Enum IRelationRule.Destination => Destination;

        [DataMember]
        public RelationCondition[] Conditions { get; set; }
    }
}
