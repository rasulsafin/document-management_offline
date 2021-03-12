using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronization
{
    public class YandexProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly YandexManager manager;
        private List<ProjectExternalDto> projects;

        public YandexProjectsSynchronizer(YandexConnectionContext context)
            => manager = context.YandexManager;

        public async Task<ProjectExternalDto> Add(ProjectExternalDto project)
        {
            var newId = Guid.NewGuid().ToString();
            project.ExternalID = newId;
            var createSuccess = await manager.Push(project, newId);
            if (!createSuccess)
                return null;

            var createdProject = await manager.Pull<ProjectExternalDto>(newId);
            await ItemsSyncHelper.UploadFiles(createdProject.Items, manager);
            return createdProject;
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            await CheckCashedElements();
            return projects.Where(p => ids.Contains(p.ExternalID)).ToList();
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await CheckCashedElements();
            return projects.Where(p => p.UpdatedAt <= date).Select(p => p.ExternalID).ToList();
        }

        public async Task<ProjectExternalDto> Remove(ProjectExternalDto project)
        {
            var deleteResult = await manager.Delete<ProjectExternalDto>(project.ExternalID);
            if (!deleteResult)
                return null;

            return project;
        }

        public async Task<ProjectExternalDto> Update(ProjectExternalDto project)
        {
            var removedObjective = await Remove(project);
            if (removedObjective == null)
                return null;

            var updated = await Add(project);
            await ItemsSyncHelper.UploadFiles(updated.Items, manager);
            return updated;
        }

        private async Task CheckCashedElements()
        {
            if (projects == null)
                projects = await manager.PullAll<ProjectExternalDto>(PathManager.GetTableDir(nameof(ProjectExternalDto)));
        }
    }
}
