using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.External.Utils;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.External.CloudBase.Synchronizers
{
    internal class ItemsSyncHelper
    {
        internal static async Task UploadFiles(ICollection<ItemExternalDto> items, ICloudManager manager, string projectName = null, bool forceUploading = false)
        {
            if (items == null)
                return;

            var remoteDirectoryName = string.IsNullOrWhiteSpace(projectName)
                ? PathManager.FILES_DIRECTORY
                : PathManager.GetFilesDirectoryForProject(projectName);

            var toUpload = forceUploading ? items : items.Where(i => string.IsNullOrWhiteSpace(i.ExternalID));

            foreach (var item in toUpload)
            {
                var directory = remoteDirectoryName;
                var folders = item.RelativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                directory = folders.Take(folders.Length - 1).Aggregate(directory, PathManager.DirectoryName).Trim('/');
                var existingRemoteFiles = await manager.GetRemoteDirectoryFiles(PathManager.GetNestedDirectory(directory));
                var itemsRemoteVersion = existingRemoteFiles.FirstOrDefault(i => i.DisplayName == item.FileName);
                if (itemsRemoteVersion?.Href != default)
                {
                    item.ExternalID = itemsRemoteVersion.Href;
                    if (!forceUploading)
                        continue;
                }

                var uploadedHref = await manager.PushFile(directory, item.FullPath);
                item.ExternalID = uploadedHref;
            }
        }

        internal static async Task<ICollection<ItemExternalDto>> GetProjectItems(string projectName, ICloudManager manager)
        {
            var resultItems = new List<ItemExternalDto>();
            var projectFilesFolder = PathManager.GetFilesDirectoryForProject(projectName);
            projectFilesFolder = PathManager.GetNestedDirectory(projectFilesFolder);
            var remoteProjectFiles = GetNestedFiles(manager, projectFilesFolder);
            await foreach (var file in remoteProjectFiles)
            {
                resultItems.Add(new ItemExternalDto
                {
                    ExternalID = file.Href,
                    RelativePath = Path.GetRelativePath(projectFilesFolder, file.Href),
                    ItemType = ItemTypeHelper.GetTypeByName(file.DisplayName),
                    UpdatedAt = file.LastModified > file.CreationDate ? file.LastModified : file.CreationDate,
                });
            }

            return resultItems;
        }

        private static async IAsyncEnumerable<CloudElement> GetNestedFiles(ICloudManager manager, string directoryPath)
        {
            var remoteProjectFiles = await manager.GetRemoteDirectoryFiles(directoryPath);

            foreach (var item in remoteProjectFiles)
            {
                if (item.IsDirectory)
                {
                    var subdirectory = PathManager.DirectoryName(directoryPath, item.DisplayName);
                    await foreach (var subdirectoryItem in GetNestedFiles(manager, subdirectory))
                        yield return subdirectoryItem;
                }
                else
                {
                    yield return item;
                }
            }
        }
    }
}
