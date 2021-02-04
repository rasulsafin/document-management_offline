using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Forge
{
    [DataContract]
    public class CloudItem<T, T2>
            where T : Bim360Object<T2>
            where T2 : AAtribute
    {
        [DataMember(EmitDefaultValue = false)]
        public string id;
        [DataMember(EmitDefaultValue = false)]
        public string type;

        [DataMember(Name = "attributes", EmitDefaultValue = false)]
        public T data;

        [JsonConstructor]
        public CloudItem(string id, T attributes)
        {
            data = attributes;
            this.id = id;
            data.ID = id;
        }

        public CloudItem(T attributes, string type)
        {
            data = attributes;
            this.id = attributes.ID;
            this.type = type;
        }
    }
}
