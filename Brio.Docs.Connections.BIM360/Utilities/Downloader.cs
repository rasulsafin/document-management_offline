using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Extensions;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal class Downloader
    {
        private readonly ItemsService itemsService;
        private readonly ObjectsService objectsService;

        public Downloader(ItemsService itemsService, ObjectsService objectsService)
        {
            this.itemsService = itemsService;
            this.objectsService = objectsService;
        }

        public async Task<FileInfo> Download(string projectID, Item item, Version version, string path)
        {
            var storage = version?.GetStorage() ??
                (await itemsService.GetVersions(projectID, item.ID))
               .Select(x => (x.Attributes.VersionNumber, storage: x.GetStorage()))
               .Where(x => x.storage != null)
               .Aggregate((max, vs) => max.VersionNumber >= vs.VersionNumber ? max : vs)
               .storage;

            if (storage != null)
            {
                var (bucketKey, hashedName) = storage.ParseStorageId();
                await objectsService.GetAsync(bucketKey, hashedName, path);
                return new FileInfo(path);
            }

            return null;
        }
    }
}
