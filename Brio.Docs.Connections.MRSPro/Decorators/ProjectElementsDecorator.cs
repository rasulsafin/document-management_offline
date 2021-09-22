using Brio.Docs.Connections.MrsPro.Interfaces;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brio.Docs.Connections.MrsPro.Services
{
    public class ProjectElementsDecorator : AObjectiveElementDecorator
    {
        private readonly IConverter<Project, ObjectiveExternalDto> dtoConverter;
        private readonly IConverter<ObjectiveExternalDto, Project> modelConverter;
        private readonly ProjectsService projectsService;

        public ProjectElementsDecorator(
            ProjectsService projectsService,
            PlansService plansService,
            IConverter<Project, ObjectiveExternalDto> dtoConverter,
            IConverter<ObjectiveExternalDto, Project> modelConverter)
        {
            this.dtoConverter = dtoConverter;
            this.modelConverter = modelConverter;
            this.projectsService = projectsService;
        }

        public override async Task<IEnumerable<IElementObject>> GetAll(DateTime date)
        {
            var listOfAllProjects = await projectsService.GetAll();
            return listOfAllProjects.Where(p => p.Ancestry != projectsService.RootPath).ToArray();
        }

        public override async Task<IEnumerable<IElementObject>> GetElementsByIds(IReadOnlyCollection<string> ids)
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

        public override async Task<IElementObject> GetElementById(string id)
            => await projectsService.TryGetById(id);

        public override async Task<IElementObject> PostElement(IElementObject element)
            => await projectsService.TryPost(element as Project);

        public override async Task<IElementObject> PatchElement(UpdatedValues valuesToPatch)
            => await projectsService.TryPatch(valuesToPatch);

        public override async Task<bool> DeleteElementById(string id)
             => await projectsService.TryDelete(id);

        public override async Task<ObjectiveExternalDto> ConvertToDto(IElementObject element)
            => await dtoConverter.Convert(element as Project);

        public override async Task<IElementObject> ConvertToModel(ObjectiveExternalDto element)
            => await modelConverter.Convert(element);

        public override async Task<IEnumerable<IElementAttachment>> GetAttachments(string id)
            => await projectsService.GetAttachments(id);
    }
}
