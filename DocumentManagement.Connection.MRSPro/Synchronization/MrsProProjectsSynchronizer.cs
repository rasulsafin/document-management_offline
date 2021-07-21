using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.General.Utils.Extensions;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ProjectsDecorator projectsService;

        public MrsProProjectsSynchronizer(ProjectsDecorator projectsService)
        {
            this.projectsService = projectsService;
        }

        public Task<ProjectExternalDto> Add(ProjectExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var projectsDtoList = new List<ProjectExternalDto>();
            var projects = await projectsService.GetElementsByIds(ids);
            foreach (var project in projects)
            {
                if (project.HasAttachments)
                    project.Attachments = await projectsService.GetAttachments(project.GetExternalId());
                projectsDtoList.AddIsNotNull(await projectsService.ConvertToDto(project));
            }

            return projectsDtoList;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            var result = await projectsService.GetAll(date);
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
