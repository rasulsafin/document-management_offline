﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronization
{
    internal class ItemsSyncHelper
    {
        internal static async Task UploadFiles(ICollection<ItemExternalDto> items, YandexManager manager)
        {
            if (items == null)
                return;

            var remoteDirectoryName = PathManager.FILES_DIRECTORY;
            var existingRemoteFiles = await manager.GetRemoteDirectoryFiles(PathManager.GetDir(remoteDirectoryName));

            foreach (var item in items.Where(i => string.IsNullOrWhiteSpace(i.ExternalID)))
            {
                var itemsRemoteVersion = existingRemoteFiles.FirstOrDefault(i => i.DisplayName == item.FileName);
                if (itemsRemoteVersion?.Href != default)
                {
                    item.ExternalID = itemsRemoteVersion.Href;
                    continue;
                }

                var uploadedHref = await manager.PushFile(remoteDirectoryName, Path.GetDirectoryName(item.FullPath), item.FileName);
                item.ExternalID = uploadedHref;
            }
        }
    }
}
