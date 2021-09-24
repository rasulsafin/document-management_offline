using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    internal enum RelationComparisonValueType
    {
        [EnumMember]
        Int,
        [EnumMember]
        Float,
        [EnumMember]
        DateTime,
    }
}
