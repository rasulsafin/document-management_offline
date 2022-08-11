using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Forge.Models.Bim360
{
    public class Mention
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
