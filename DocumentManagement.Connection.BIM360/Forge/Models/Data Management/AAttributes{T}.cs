using System.Runtime.Serialization;

namespace Brio.Docs.Connection.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public abstract class AAttributes<T>
        where T : Extension
    {
        [DataMember(Name = "extension")]
        public T Extension { get; set; }
    }
}
