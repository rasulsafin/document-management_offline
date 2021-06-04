using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ProjectService projectService;

        public MrsProProjectsSynchronizer(ProjectService projectService)
        {
            this.projectService = projectService;
        }

        public Task<ProjectExternalDto> Add(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            var result = await projectService.GetProjects();
            return result.Select(x => x.Id).ToArray();
        }

        public Task<ProjectExternalDto> Remove(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectExternalDto> Update(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }
    }
}
