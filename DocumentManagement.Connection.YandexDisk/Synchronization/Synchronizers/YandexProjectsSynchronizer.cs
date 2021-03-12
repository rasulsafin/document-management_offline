using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronization.Synchronizers
{
    public class YandexProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly YandexManager manager;

        public YandexProjectsSynchronizer(YandexConnectionContext context)
            => manager = context.YandexManager;

        public async Task<ProjectExternalDto> Add(ProjectExternalDto project)
        {
            var newId = Guid.NewGuid().ToString();
            var createSuccess = await manager.Push(project, newId);
            if (!createSuccess)
                return null;

            var createdProject = await manager.Pull<ProjectExternalDto>(newId);
            return createdProject;
        }

        public async Task<ProjectExternalDto> Remove(ProjectExternalDto project)
        {
            var deleteResult = await manager.Delete<ObjectiveExternalDto>(project.ExternalID);
            if (!deleteResult)
                return null;

            return project;
        }

        public async Task<ProjectExternalDto> Update(ProjectExternalDto project)
        {
            var removedObjective = await Remove(project);
            if (removedObjective == null)
                return null;

            return await Add(project);
        }
    }
}
