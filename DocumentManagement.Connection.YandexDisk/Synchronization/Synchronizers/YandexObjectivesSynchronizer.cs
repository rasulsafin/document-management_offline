using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronization.Synchronizers
{
    public class YandexObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly YandexManager manager;

        public YandexObjectivesSynchronizer(YandexConnectionContext context)
            => manager = context.YandexManager;

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var newId = Guid.NewGuid().ToString();
            obj.ExternalID = newId;
            var createSuccess = await manager.Push(obj, newId);
            if (!createSuccess)
                return null;

            var createdObjective = await manager.Pull<ObjectiveExternalDto>(newId);
            await UploadFiles(createdObjective.Items);

            return createdObjective;
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            var deleteResult = await manager.Delete<ObjectiveExternalDto>(obj.ExternalID);
            if (!deleteResult)
                return null;

            obj.Status = ObjectiveStatus.Ready;
            return obj;
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var removedObjective = await Remove(obj);
            if (removedObjective == null)
                return null;

            var updated = await Add(obj);
            await UploadFiles(updated.Items);
            return updated;
        }

        private async Task UploadFiles(ICollection<ItemExternalDto> items)
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
