using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class DMItemExtra : DMItem
    {
        [JsonProperty]
        public List<DMFile> Documents { get; set; }
        [JsonProperty]
        public List<DMFile> Attachments { get; set; }
    }
}