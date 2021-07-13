using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class IssuesDecorator : AElementDecorator
    {
        private readonly IConverter<Issue, ObjectiveExternalDto> dtoConverter;
        private readonly IConverter<ObjectiveExternalDto, Issue> modelConverter;
        private readonly IssuesService issuesService;

        public IssuesDecorator(IssuesService issuesService,
            IConverter<Issue, ObjectiveExternalDto> dtoConverter,
            IConverter<ObjectiveExternalDto, Issue> modelConverter)
        {
            this.dtoConverter = dtoConverter;
            this.modelConverter = modelConverter;
            this.issuesService = issuesService;
        }

        public override async Task<IEnumerable<IElement>> GetAll(DateTime date)
            => await issuesService.GetAll(date);

        public override async Task<IEnumerable<IElement>> GetElementsByIds(IReadOnlyCollection<string> ids)
            => await issuesService.TryGetByIds(ids);

        public override async Task<IElement> GetElementById(string id)
            => await issuesService.TryGetById(id);

        public override async Task<IElement> PostElement(IElement element)
            => await issuesService.TryPost(element as Issue);

        public override async Task<IElement> PatchElement(UpdatedValues valuesToPatch)
            => await issuesService.TryPatch(valuesToPatch);

        public override async Task<bool> DeleteElementById(string id)
            => await issuesService.TryDelete(id);

        public override async Task<ObjectiveExternalDto> ConvertToDto(IElement element)
            => element == null ? null : await dtoConverter.Convert(element as Issue);

        public override async Task<IElement> ConvertToModel(ObjectiveExternalDto element)
            => element == null ? null : await modelConverter.Convert(element);
    }
}
