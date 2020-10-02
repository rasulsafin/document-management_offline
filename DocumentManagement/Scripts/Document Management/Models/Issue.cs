using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class Issue : DMItemExtra, IGuidLinkable
    {
        [JsonProperty]
        public DMItem DMParent { get; set; }
        [JsonProperty]
        public bool IsFixed { get; set; }
        [JsonProperty]
        public dynamic Author { get; set; }
        [JsonProperty]
        public dynamic Contractor { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string Commentary { get; set; }
        [JsonProperty]
        public DateTime Detect { get; set; }
        [JsonProperty]
        public DateTime AmendTo { get; set; }

        public List<string> Links { get; set; }

    }
}
