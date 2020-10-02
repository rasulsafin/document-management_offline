using Newtonsoft.Json;
using System;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class DMFile : DMItem
    {
        [JsonProperty]
        public string Path { get; set; }
    }
}