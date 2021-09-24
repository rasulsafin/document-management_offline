using System.Runtime.Serialization;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Models.StatusRelations
{
    [DataContract]
    internal enum RelationComparisonType
    {
        [EnumMember]
        Equals,
        [EnumMember]
        More,
        [EnumMember]
        Less,
    }
}
