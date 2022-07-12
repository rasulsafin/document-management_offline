using System.Runtime.Serialization;

namespace Brio.Docs.Connections.Bim360.Forge.Models
{
    [DataContract]
    public class DataContainer<T>
    {
        public DataContainer(T data)
            => Data = data;

        [DataMember(Name = "data")]
        public T Data { get; set; }
    }
}