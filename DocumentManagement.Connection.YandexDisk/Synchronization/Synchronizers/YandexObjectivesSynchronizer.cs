using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronization
{
    public class YandexObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly YandexManager manager;
        private List<ObjectiveExternalDto> objectives;

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

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await CheckCashedElements();
            return objectives.Where(o => ids.Contains(o.ExternalID)).ToList();
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await CheckCashedElements();
            return objectives
                .Where(o => o.UpdatedAt != default)
                .Where(o => o.UpdatedAt <= date)
                .Select(o => o.ExternalID).ToList();
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

        private async Task CheckCashedElements()
        {
            if (objectives == null)
                objectives = await manager.PullAll<ObjectiveExternalDto>(PathManager.GetTableDir(nameof(ObjectiveExternalDto)));
        }
    }
}
