using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var existingRemoteFiles = await manager.GetRemoteDirectoryFiles(PathManager.GetNestedDirectory(remoteDirectoryName));

            var toUpload = forceUploading ? items : items.Where(i => string.IsNullOrWhiteSpace(i.ExternalID));

            foreach (var item in toUpload)
            {
                var itemsRemoteVersion = existingRemoteFiles.FirstOrDefault(i => i.DisplayName == item.FileName);
                if (itemsRemoteVersion?.Href != default)
                {
                    item.ExternalID = itemsRemoteVersion.Href;
                    if (!forceUploading)
                        continue;
                }

                var uploadedHref = await manager.PushFile(remoteDirectoryName, item.FullPath);
                item.ExternalID = uploadedHref;
            }
        }

        internal static async Task<ICollection<ItemExternalDto>> GetProjectItems(string projectName, ICloudManager manager)
        {
            var resultItems = new List<ItemExternalDto>();
            var projectFilesFolder = PathManager.GetFilesDirectoryForProject(projectName);
            var remoteProjectFiles = await manager.GetRemoteDirectoryFiles(PathManager.GetNestedDirectory(projectFilesFolder));
            foreach (var file in remoteProjectFiles.Where(f => !f.IsDirectory))
            {
                resultItems.Add(new ItemExternalDto
                {
                    ExternalID = file.Href,
                    RelativePath = file.DisplayName,
                    ItemType = ItemTypeHelper.GetTypeByName(file.DisplayName),
                    UpdatedAt = file.LastModified > file.CreationDate ? file.LastModified : file.CreationDate,
                });
            }

            return resultItems;
        }
    }
}
