using Newtonsoft.Json;
using System;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class DMItem
    {        
        [JsonProperty]
        public dynamic ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
    }
}
