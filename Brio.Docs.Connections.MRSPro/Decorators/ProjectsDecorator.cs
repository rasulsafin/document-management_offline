using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.MrsPro.Interfaces;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.MrsPro.Services
{
    public class ProjectsDecorator : IElementDecorator<Project, Plan>, IElementConvertible<ProjectExternalDto, Project>
    {
        private readonly IConverter<Project, ProjectExternalDto> dtoConverter;
        private readonly IConverter<ProjectExternalDto, Project> modelConverter;
        private readonly ProjectsService projectsService;

        public ProjectsDecorator(ProjectsService projectsService,
            IConverter<Project, ProjectExternalDto> dtoConverter,
            IConverter<ProjectExternalDto, Project> modelConverter)
        {
            this.dtoConverter = dtoConverter;
            this.modelConverter = modelConverter;
            this.projectsService = projectsService;
        }

        public async Task<bool> DeleteElementById(string id)
             => await projectsService.TryDelete(id);

        public async Task<IEnumerable<Project>> GetAll(DateTime date)
        {
            var listOfAllProjects = await projectsService.GetAll();
            return listOfAllProjects.Where(p => p.Ancestry == projectsService.RootPath).ToArray();
        }

        public async Task<IEnumerable<Plan>> GetAttachments(string id)
            => await projectsService.GetAttachments(id);

        public async Task<Project> GetElementById(string id)
            => await projectsService.TryGetById(id);

        public async Task<IEnumerable<Project>> GetElementsByIds(IReadOnlyCollection<string> ids)
        {
            var projects = await projectsService.TryGetByIds(ids);
            var attachments = await projectsService.TryGetAttachmentInfoByIds(ids);
            return projects.Join(
                attachments,
                project => project.Id,
                attachment => attachment.ProjectId,
                (project, attachment) =>
                {
                    project.HasAttachments = attachment.HasDocumentation;
                    return project;
                });
        }

        public async Task<Project> PatchElement(UpdatedValues valuesToPatch)
            => await projectsService.TryPatch(valuesToPatch);

        public async Task<Project> PostElement(Project project)
            => await projectsService.TryPost(project);

        public async Task<ProjectExternalDto> ConvertToDto(Project project)
            => project == null ? null : await dtoConverter.Convert(project);

        public async Task<Project> ConvertToModel(ProjectExternalDto project)
            => project == null ? null : await modelConverter.Convert(project);
    }
}
