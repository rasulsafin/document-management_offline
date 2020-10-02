using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class Volume
    {
        [JsonProperty]
        public float Plan { get; set; }
        [JsonProperty]
        public float Fact { get; set; }
        [JsonProperty]
        public List<(string guid, string constractor, float volume)> Progress { get; set; }
    }
}

