using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class ProjectElementsService : AElementService
    {
        private readonly IConverter<Project, ObjectiveExternalDto> dtoConverter;
        private readonly IConverter<ObjectiveExternalDto, Project> modelConverter;
        private readonly ProjectsService projectsService;

        public ProjectElementsService(MrsProHttpConnection connection,
            IConverter<Project, ObjectiveExternalDto> dtoConverter,
            IConverter<ObjectiveExternalDto, Project> modelConverter,
            ProjectsService projectsService)
            : base(connection)
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

        public override async Task<IEnumerable<IElement>> TryGetByIds(IReadOnlyCollection<string> ids)
            => await projectsService.TryGetByIds(ids);

        public override async Task<IElement> TryGetById(string id)
            => await projectsService.TryGetById(id);

        public override Task<IElement> TryPost(IElement element)
        {
            throw new NotImplementedException();
        }

        public override Task<IElement> TryPatch(UpdatedValues valuesToPatch)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> TryDelete(string id)
        {
            throw new NotImplementedException();
        }

        public override async Task<ObjectiveExternalDto> ConvertToDto(IElement element)
            => await dtoConverter.Convert(element as Project);

        public override async Task<IElement> ConvertToModel(ObjectiveExternalDto element)
            => await modelConverter.Convert(element);
    }
}
