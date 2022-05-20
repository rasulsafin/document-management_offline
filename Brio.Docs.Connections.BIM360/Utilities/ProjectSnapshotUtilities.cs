using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Extensions;
using Brio.Docs.Connections.Bim360.Forge;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Properties;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Version = Brio.Docs.Connections.Bim360.Forge.Models.DataManagement.Version;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot
{
    internal class ProjectSnapshotUtilities
    {
        private readonly FoldersService foldersService;

        public ProjectSnapshotUtilities(FoldersService foldersService)
        {
            this.foldersService = foldersService;
        }

        public async Task DownloadFoldersInfo(ProjectSnapshot snapshot)
        {
            List<(Item item, Version version)> items = await foldersService
               .GetAllSynchronizingItems(snapshot.ID, snapshot.TopFolders.Select(x => x.ID).ToAsyncEnumerable())
               .ToListAsync();

            snapshot.Items = items
               .Where(x => !IsMetaFile(x.item) && !IsMetaFile(x.version))
               .ToDictionary(x => x.item.ID, x => new ItemSnapshot(x.item, x.version));
            snapshot.UploadFolderID = GetMrsFolderId(snapshot.TopFolders, items);
        }

        private static bool IsMetaFile(Item item)
        {
            return IsMetaFile(item.Attributes.DisplayName);
        }

        private static bool IsMetaFile(Version version)
        {
            return IsMetaFile(version.Attributes.Name);
        }

        private static bool IsMetaFile(string name)
        {
            return string.Equals(name, Resources.UploadMrsFileName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, Resources.SynchronizeMrsFileName, StringComparison.InvariantCultureIgnoreCase);
        }

        private async IAsyncEnumerable<ItemSnapshot> GetItems(Project p, IEnumerable<string> topFolderIds)
        {
            IAsyncEnumerable<(Item item, Version version)> items =
                foldersService.GetAllSynchronizingItems(p.ID, topFolderIds.ToAsyncEnumerable());

            await foreach ((Item item, Version version) iv in items)
                yield return new ItemSnapshot(iv.item, iv.version);
        }

        private string GetMrsFolderId(
            IReadOnlyCollection<Folder> topFolders,
            IEnumerable<(Item item, Version version)> items)
        {
            Folder uploadFolder = topFolders.FirstOrDefault(
                x => x.Attributes.DisplayName == Constants.DEFAULT_PROJECT_FILES_FOLDER_NAME ||
                    x.Attributes.Extension.Data.VisibleTypes.Contains(Constants.AUTODESK_ITEM_FILE_TYPE));
            uploadFolder ??= topFolders.First();

            foreach ((Item item, Version version) iv in items)
            {
                if (string.Equals(
                        iv.item.Attributes.DisplayName,
                        Resources.UploadMrsFileName,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        iv.version.Attributes.Name,
                        Resources.UploadMrsFileName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return iv.item.Relationships.Parent.Data.ID;
                }
            }

            return uploadFolder.ID;
        }
    }
}
