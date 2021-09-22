using Brio.Docs.Connection.MrsPro.Services;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connection.MrsPro.Extensions;
using Brio.Docs.General.Utils.Extensions;

namespace Brio.Docs.Connection.MrsPro
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
