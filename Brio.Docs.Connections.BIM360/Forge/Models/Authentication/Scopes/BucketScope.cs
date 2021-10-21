using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Authentication.Scopes
{
    internal enum BucketScope
    {
        [EnumMember(Value = "bucket:create")]
        Create,
        [EnumMember(Value = "bucket:read")]
        Read,
        [EnumMember(Value = "bucket:update")]
        Update,
        [EnumMember(Value = "bucket:delete")]
        Delete,
    }
}
