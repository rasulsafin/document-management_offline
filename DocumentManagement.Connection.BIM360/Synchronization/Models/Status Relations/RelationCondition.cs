using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    internal class RelationCondition
    {
        [DataMember]
        public string PropertyName { get; set; }

        [DataMember]
        public ComparisonObjectType? ObjectType { get; set; }

        [DataMember]
        public RelationComparisonType? ComparisonType { get; set; }

        [DataMember]
        public RelationComparisonValueType? ValueType { get; set; }

        [DataMember]
        public object[] Values { get; set; }
    }
}
