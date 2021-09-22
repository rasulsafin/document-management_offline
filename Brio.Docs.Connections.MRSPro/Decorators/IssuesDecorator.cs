using Brio.Docs.Connections.MrsPro.Interfaces;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brio.Docs.Connections.MrsPro.Services
{
    public class IssuesDecorator : AObjectiveElementDecorator
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

        public override async Task<IEnumerable<IElementObject>> GetAll(DateTime date)
            => await issuesService.GetAll(date);

        public override async Task<IEnumerable<IElementObject>> GetElementsByIds(IReadOnlyCollection<string> ids)
        {
            var issues = await issuesService.TryGetByIds(ids);
            var attachments = await issuesService.TryGetAttachmentInfoByIds(ids);
            return issues.Join(
                attachments,
                issue => issue.Id,
                attachment => attachment.TaskId,
                (issue, attachment) =>
                {
                    issue.HasAttachments = attachment.HasImage;
                    return issue;
                });
        }

        public override async Task<IElementObject> GetElementById(string id)
            => await issuesService.TryGetById(id);

        public override async Task<IElementObject> PostElement(IElementObject element)
            => await issuesService.TryPost(element as Issue);

        public override async Task<IElementObject> PatchElement(UpdatedValues valuesToPatch)
            => await issuesService.TryPatch(valuesToPatch);

        public override async Task<bool> DeleteElementById(string id)
            => await issuesService.TryDelete(id);

        public override async Task<ObjectiveExternalDto> ConvertToDto(IElementObject element)
            => element == null ? null : await dtoConverter.Convert(element as Issue);

        public override async Task<IElementObject> ConvertToModel(ObjectiveExternalDto element)
            => element == null ? null : await modelConverter.Convert(element);

        public override async Task<IEnumerable<IElementAttachment>> GetAttachments(string id)
            => await issuesService.GetAttachments(id);
    }
}
