using Brio.Docs.Connections.Bim360.Synchronization.Models;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Client.Dtos;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class IfcConfigUtilities
    {
        private readonly Downloader downloader;

        public IfcConfigUtilities(Downloader downloader)
            => this.downloader = downloader;

        public async Task<IfcConfig> GetConfig(
            ObjectiveExternalDto obj,
            ProjectSnapshot project,
            ItemSnapshot itemSnapshot)
        {
            if (string.IsNullOrWhiteSpace(obj.Location?.Item?.FileName))
                return null;

            var configName = obj.Location.Item.FileName + MrsConstants.CONFIG_EXTENSION;
            var config = project.Items
               .Where(
                    x => x.Value.Entity.Relationships.Parent.Data.ID ==
                        itemSnapshot.Entity.Relationships.Parent.Data.ID)
               .FirstOrDefault(
                    x => string.Equals(
                        x.Value.Entity.Attributes.DisplayName,
                        configName,
                        StringComparison.OrdinalIgnoreCase))
               .Value;

            if (config != null)
            {
                var downloadedConfig = await downloader.Download(
                    project.ID,
                    config.Entity,
                    config.Version,
                    Path.GetTempFileName());

                if (downloadedConfig?.Exists ?? false)
                {
                    var ifcConfig = JsonConvert.DeserializeObject<IfcConfig>(
                        await File.ReadAllTextAsync(downloadedConfig.FullName));
                    downloadedConfig.Delete();
                    return ifcConfig;
                }
            }

            return default;
        }
    }
}
