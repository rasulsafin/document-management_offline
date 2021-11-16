using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Properties;
using Brio.Docs.Connections.Bim360.Synchronization.Models;
using Brio.Docs.Connections.Bim360.Synchronization.Models.StatusRelations;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class ConfigurationUtilities
    {
        private readonly Downloader downloader;

        public ConfigurationUtilities(Downloader downloader)
            => this.downloader = downloader;

        public static StatusesRelations GetDefaultStatusesConfig()
        {
            var json = Resources.statuses;
            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("Resource not found");
            return JsonConvert.DeserializeObject<StatusesRelations>(json);
        }

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

            return await GetConfigFromRemote<IfcConfig>(project, config);
        }

        public async Task<StatusesRelations> GetStatusesConfig(ProjectSnapshot project)
        {
            var configName = MrsConstants.STATUSES_CONFIG_NAME + MrsConstants.CONFIG_EXTENSION;
            var config = project.Items
               .FirstOrDefault(
                    x => string.Equals(
                        x.Value.Entity.Attributes.DisplayName,
                        configName,
                        StringComparison.OrdinalIgnoreCase))
               .Value;

            return await GetConfigFromRemote<StatusesRelations>(project, config);
        }

        private async Task<T> GetConfigFromRemote<T>(ProjectSnapshot project, ItemSnapshot config)
            where T : class
        {
            if (config != null)
            {
                var downloadedConfig = await downloader.Download(
                    project.ID,
                    config.Entity,
                    config.Version,
                    Path.GetTempFileName());

                if (downloadedConfig?.Exists ?? false)
                {
                    var ifcConfig = JsonConvert.DeserializeObject<T>(
                        await File.ReadAllTextAsync(downloadedConfig.FullName));
                    downloadedConfig.Delete();
                    return ifcConfig;
                }
            }

            return null;
        }
    }
}
