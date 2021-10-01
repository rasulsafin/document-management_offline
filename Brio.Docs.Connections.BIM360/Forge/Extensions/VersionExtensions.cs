using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Utils.Extensions;

namespace Brio.Docs.Connections.Bim360.Forge.Extensions
{
    public static class VersionExtensions
    {
        public static Version Max(this IEnumerable<Version> versions)
            => versions?.Aggregate(
                (max, version) => max.Attributes.VersionNumber >= version.Attributes.VersionNumber ? max : version);

        public static StorageObject GetStorage(this Version version)
            => version?.Relationships?.Storage?.Data
               .ToObject<StorageObject, StorageObject.StorageObjectAttributes,
                    StorageObject.StorageObjectRelationships>();
    }
}
