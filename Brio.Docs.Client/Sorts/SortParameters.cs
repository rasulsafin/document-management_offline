using System.Collections.Generic;
using Newtonsoft.Json;

namespace Brio.Docs.Client.Sorts
{
    public class SortParameters
    {
        [JsonProperty("sort")]
        public List<SortParameter> Sortings { get; set; }
    }
}
