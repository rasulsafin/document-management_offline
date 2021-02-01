using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class Project : DMItemExtra
    {
        [JsonProperty]
        public List<Job> Jobs { get; set; }
        [JsonProperty]
        public List<Issue> Issues { get; set; }
        [JsonProperty]
        public List<Blueprint> Blueprints { get; set; }
        [JsonProperty]
        public List<DMFile> Ifcs { get; set; }
        [JsonProperty]
        public List<DMAction> Actions { get; set; }
    }
}