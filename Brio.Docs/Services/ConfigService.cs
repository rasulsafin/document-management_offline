using System.IO;
using Brio.Docs.Dtos;
using Newtonsoft.Json;

namespace Brio.Docs.Services
{
    public class ConfigService : IConfigService
    {
        public ConfigService()
        {
            if (File.Exists("Config.JSON"))
            {
                var text = File.ReadAllText("Config.JSON");
                Config = JsonConvert.DeserializeObject<ConfigDto>(text);
            }
            else
            {
                Config = new ConfigDto();
                File.WriteAllText("Config.JSON", JsonConvert.SerializeObject(Config));
            }
        }

        public ConfigDto Config { get; set; }
    }
}
