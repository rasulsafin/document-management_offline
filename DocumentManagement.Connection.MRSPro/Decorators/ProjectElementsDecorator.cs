using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectElementsDecorator : AElementDecorator
    {
        private readonly IConverter<Project, ObjectiveExternalDto> dtoConverter;
        private readonly IConverter<ObjectiveExternalDto, Project> modelConverter;
        private readonly ProjectsService projectsService;

        public ProjectElementsDecorator
            (ProjectsService projectsService,
            IConverter<Project, ObjectiveExternalDto> dtoConverter,
            IConverter<ObjectiveExternalDto, Project> modelConverter)
        {
            this.dtoConverter = dtoConverter;
            this.modelConverter = modelConverter;
            this.projectsService = projectsService;
        }

        public override async Task<IEnumerable<IElement>> GetAll(DateTime date)
        {
            var listOfAllProjects = await projectsService.GetListOfProjects();
            return listOfAllProjects.Where(p => p.Ancestry != projectsService.RootPath).ToArray();
        }

        public override async Task<IEnumerable<IElement>> GetElementsByIds(IReadOnlyCollection<string> ids)
            => await projectsService.TryGetByIds(ids);

        public override async Task<IElement> GetElementById(string id)
            => await projectsService.TryGetById(id);

        public override Task<IElement> PostElement(IElement element)
        {
            throw new NotImplementedException();
        }

        public override Task<IElement> PatchElement(UpdatedValues valuesToPatch)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteElementById(string id)
        {
            throw new NotImplementedException();
        }

        public override async Task<ObjectiveExternalDto> ConvertToDto(IElement element)
            => await dtoConverter.Convert(element as Project);

        public override async Task<IElement> ConvertToModel(ObjectiveExternalDto element)
            => await modelConverter.Convert(element);
    }
}
