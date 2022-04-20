using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Properties;

namespace Brio.Docs.Connections.Bim360.Extensions
{
    public static class FolderServiceExtensions
    {
        private enum SynchronizeFolder
        {
            DoNotSynchronize,
            SynchronizeFolder,
            SynchronizeFolderWithSubFolders,
        }

        public static IAsyncEnumerable<(Item item, Version version)> GetAllSynchronizingItems(
            this FoldersService foldersService,
            string projectID,
            IAsyncEnumerable<string> folderIds)
        {
            return foldersService.GetAllSynchronizingItems(projectID, folderIds, false);
        }

        private static async IAsyncEnumerable<(Item item, Version version)> GetAllSynchronizingItems(
            this FoldersService foldersService,
            string projectID,
            IAsyncEnumerable<string> folderIds,
            bool synchronizeItems)
        {
            await foreach (string folderId in folderIds)
            {
                bool needToSynchronize = synchronizeItems;

                if (!needToSynchronize)
                {
                    SynchronizeFolder synchronize = await foldersService.NeedSynchronize(projectID, folderId);
                    needToSynchronize = synchronize != SynchronizeFolder.DoNotSynchronize;
                    synchronizeItems = synchronize == SynchronizeFolder.SynchronizeFolderWithSubFolders;
                }

                if (needToSynchronize)
                {
                    await foreach (var item in foldersService.GetItemsAsync(projectID, folderId))
                        yield return item;
                }

                var subfoldersItems = foldersService.GetAllSynchronizingItems(
                    projectID,
                    foldersService.GetFoldersAsync(projectID, folderId).Select(x => x.ID),
                    synchronizeItems);

                await foreach ((Item item, Version version) item in subfoldersItems)
                    yield return item;
            }
        }

        private static async Task<SynchronizeFolder> NeedSynchronize(
            this FoldersService foldersService,
            string projectID,
            string folderId)
        {
            string path = DataMemberUtilities.GetPath<Version.VersionAttributes>(x => x.DisplayName);
            Filter filter = new (path, Resources.SynchronizeMrsFileName, Resources.UploadMrsFileName);
            List<(Item item, Version version)> files = await foldersService
               .GetItemsAsync(projectID, folderId, new IQueryParameter[] { filter })
               .ToListAsync();
            return files.Any(x => x.item.Attributes.DisplayName == Resources.SynchronizeMrsFileName)
                ? SynchronizeFolder.SynchronizeFolderWithSubFolders
                : files.Count > 0
                    ? SynchronizeFolder.SynchronizeFolder
                    : SynchronizeFolder.DoNotSynchronize;
        }
    }
}
