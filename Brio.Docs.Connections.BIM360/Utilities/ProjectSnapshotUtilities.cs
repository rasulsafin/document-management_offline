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
        private readonly ProjectsService projectsService;

        public ProjectSnapshotUtilities(ProjectsService projectsService, FoldersService foldersService)
        {
            this.projectsService = projectsService;
            this.foldersService = foldersService;
        }

        public async Task<ProjectSnapshot> GetFullProjectSnapshot(KeyValuePair<string, HubSnapshot> hub, Project p)
        {
            List<Folder> topFolders = await projectsService.GetTopFoldersAsync(hub.Key, p.ID);

            if (!topFolders.Any())
                return null;

            List<(Item item, Version version)> items = await foldersService
               .GetAllSynchronizingItems(p.ID, topFolders.Select(x => x.ID).ToAsyncEnumerable())
               .ToListAsync();

            ProjectSnapshot projectSnapshot = new (p, hub.Value)
            {
                Items = items
                   .Where(x => !IsMetaFile(x.item) && !IsMetaFile(x.version))
                   .ToDictionary(x => x.item.ID, x => new ItemSnapshot(x.item, x.version)),
                MrsFolderID = GetMrsFolderId(topFolders, items),
            };
            return projectSnapshot;
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
            return name == Resources.UploadMrsFileName ||
                name == Resources.SynchronizeMrsFileName ||
                name.EndsWith(MrsConstants.CONFIG_EXTENSION, StringComparison.OrdinalIgnoreCase);
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
                if (iv.item.Attributes.DisplayName == Resources.UploadMrsFileName ||
                    iv.version.Attributes.Name == Resources.UploadMrsFileName)
                {
                    return iv.item.Relationships.Parent.Data.ID;
                }
            }

            return uploadFolder.ID;
        }
    }
}
