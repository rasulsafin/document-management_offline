using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models.DataManagement
{
    [DataContract]
    public abstract class AAttributes<T>
        where T : Extension
    {
        [DataMember(Name = "extension")]
        public T Extension { get; set; }
    }
}
