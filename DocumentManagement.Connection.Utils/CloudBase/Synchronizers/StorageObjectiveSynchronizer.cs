using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Utils.CloudBase.Synchronizers
{
    public class StorageObjectiveSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly ICloudManager manager;
        private List<ObjectiveExternalDto> objectives;
        private List<ProjectExternalDto> projects;

        public StorageObjectiveSynchronizer(ICloudManager manager)
            => this.manager = manager;

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var newId = Guid.NewGuid().ToString();
            obj.ExternalID = newId;
            var createdObjective = await PushObjective(obj, newId);

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
                .Where(o => o.UpdatedAt >= date)
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
            var updated = await PushObjective(obj);
            return updated;
        }

        private async Task<ObjectiveExternalDto> PushObjective(ObjectiveExternalDto obj, string newId = null)
        {
            newId ??= obj.ExternalID;
            await ItemsSyncHelper.UploadFiles(
                obj.Items,
                manager,
                projects.FirstOrDefault(x => x.ExternalID == obj.ProjectExternalID)?.Title);
            obj.Items = obj.Items.Where(x => !string.IsNullOrWhiteSpace(x.ExternalID)).ToList();
            UpdatedTimeUtilities.UpdateTime(obj);
            var createSuccess = await manager.Push(obj, newId);
            if (!createSuccess)
                return null;

            var createdObjective = await manager.Pull<ObjectiveExternalDto>(newId);
            return createdObjective;
        }

        private async Task CheckCashedElements()
        {
            objectives ??= await manager.PullAll<ObjectiveExternalDto>(PathManager.GetTableDir(nameof(ObjectiveExternalDto)));
            projects ??= await manager.PullAll<ProjectExternalDto>(PathManager.GetTableDir(nameof(ProjectExternalDto)));
        }
    }
}
