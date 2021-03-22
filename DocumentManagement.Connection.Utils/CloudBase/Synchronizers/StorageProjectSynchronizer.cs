using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Utils.CloudBase.Synchronizers
{
    public class StorageProjectSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ICloudManager manager;
        private List<ProjectExternalDto> projects;

        public StorageProjectSynchronizer(ICloudManager manager)
            => this.manager = manager;

        public async Task<ProjectExternalDto> Add(ProjectExternalDto project)
        {
            var newId = Guid.NewGuid().ToString();
            project.ExternalID = newId;
            var createdProject = await PushProject(project, newId);

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
            return projects.Where(p => p.UpdatedAt >= date).Select(p => p.ExternalID).ToList();
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
            var updated = await PushProject(project);
            return updated;
        }

        private async Task<ProjectExternalDto> PushProject(ProjectExternalDto project, string newId = null)
        {
            newId ??= project.ExternalID;
            await ItemsSyncHelper.UploadFiles(project.Items, manager);
            var createSuccess = await manager.Push(project, newId);
            if (!createSuccess)
                return null;

            var createdProject = await manager.Pull<ProjectExternalDto>(newId);
            return createdProject;
        }

        private async Task CheckCashedElements()
        {
            if (projects == null)
                projects = await manager.PullAll<ProjectExternalDto>(PathManager.GetTableDir(nameof(ProjectExternalDto)));
        }
    }
}
