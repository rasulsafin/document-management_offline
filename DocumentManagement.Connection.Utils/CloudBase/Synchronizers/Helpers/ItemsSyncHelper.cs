﻿using Brio.Docs.Interface.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brio.Docs.Connections.Utils.CloudBase.Synchronizers
{
    internal class ItemsSyncHelper
    {
        private static string projectFilesFormat = $"{PathManager.FILES_DIRECTORY}/Project{PathManager.FILES_DIRECTORY}/{{0}}";

        internal static async Task UploadFiles(ICollection<ItemExternalDto> items, ICloudManager manager, string projectName = null)
        {
            if (items == null)
                return;

            var remoteDirectoryName = string.IsNullOrWhiteSpace(projectName)
                ? PathManager.FILES_DIRECTORY
                : string.Format(projectFilesFormat, projectName);

            var existingRemoteFiles = await manager.GetRemoteDirectoryFiles(PathManager.GetNestedDirectory(remoteDirectoryName));

            foreach (var item in items.Where(i => string.IsNullOrWhiteSpace(i.ExternalID)))
            {
                var itemsRemoteVersion = existingRemoteFiles.FirstOrDefault(i => i.DisplayName == item.FileName);
                if (itemsRemoteVersion?.Href != default)
                {
                    item.ExternalID = itemsRemoteVersion.Href;
                    continue;
                }

                var uploadedHref = await manager.PushFile(remoteDirectoryName, item.FullPath);
                item.ExternalID = uploadedHref;
            }
        }

        internal static async Task<ICollection<ItemExternalDto>> GetProjectItems(string projectName, ICloudManager manager)
        {
            var resultItems = new List<ItemExternalDto>();
            var projectFilesFolder = string.Format(projectFilesFormat, projectName);
            var remoteProjectFiles = await manager.GetRemoteDirectoryFiles(PathManager.GetNestedDirectory(projectFilesFolder));
            foreach (var file in remoteProjectFiles.Where(f => !f.IsDirectory))
            {
                resultItems.Add(new ItemExternalDto
                {
                    ExternalID = file.Href,
                    FileName = file.DisplayName,
                    ItemType = ItemTypeHelper.GetTypeByName(file.DisplayName),
                });
            }

            return resultItems;
        }
    }
}
