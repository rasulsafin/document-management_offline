using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronization
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
            await ItemsSyncHelper.UploadFiles(createdObjective.Items, manager);

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
            await ItemsSyncHelper.UploadFiles(updated.Items, manager);
            return updated;
        }
    }
}
