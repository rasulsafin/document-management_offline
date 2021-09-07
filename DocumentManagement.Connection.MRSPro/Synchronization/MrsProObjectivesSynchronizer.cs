using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.General.Utils.Extensions;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly AObjectiveElementDecorator issuesService;
        private readonly AObjectiveElementDecorator projectsService;

        private readonly IConverter<string, (string id, string type)> idConverter;

        public MrsProObjectivesSynchronizer(IssuesDecorator issuesService,
            ProjectElementsDecorator projectsService,
            IConverter<string, (string id, string type)> idConverter)
        {
            this.issuesService = issuesService;
            this.projectsService = projectsService;
            this.idConverter = idConverter;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var service = GetService(obj.ObjectiveType.ExternalId);
            var element = await service.ConvertToModel(obj);
            var result = await service.PostElement(element);
            return await service.ConvertToDto(result);
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var objectives = new List<ObjectiveExternalDto>();

            var objectiveGroups = ids.Select(id => idConverter.Convert(id).Result).GroupBy(t => t.type);
            foreach (var group in objectiveGroups)
            {
                var service = GetService(group.Key);
                var trueIds = group.Select(x => x.id).ToArray();
                var stepCount = 0;
                var stepValue = 100;
                var trueIdsQueue = trueIds.Take(stepValue);
                while (trueIdsQueue.Any())
                {
                    var result = await service.GetElementsByIds(trueIdsQueue.ToList());
                    foreach (var objective in result)
                    {
                        if (objective.HasAttachments)
                            objective.Attachments = await service.GetAttachments(objective.GetExternalId());
                        objectives.AddIsNotNull(await service.ConvertToDto(objective));
                    }

                    stepCount += stepValue;
                    trueIdsQueue = trueIds.Skip(stepCount).Take(stepValue);
                }
            }

            return objectives;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            var projectIds = await GetUpdatedIdsFromService(projectsService, date);
            var objectiveIds = await GetUpdatedIdsFromService(issuesService, date);

            return projectIds.Union(objectiveIds).ToArray();
        }

        public async Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            (var trueId, var type) = await idConverter.Convert(obj.ExternalID);

            if (trueId == null)
                throw new Exception("Wrong id value");

            var result = await GetService(type).DeleteElementById(trueId);

            return result ? obj : null;
        }

        public async Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            var service = GetService(obj.ObjectiveType.ExternalId);

            var element = await service.ConvertToModel(obj);
            var updatedValues = new UpdatedValues { Ids = new[] { element.Id } };
            var type = element.GetType();
            var propertiesToPatch = type.GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(CanBePatchedAttribute)))
                .Select(p => new Patch()
                    {
                        Path = $"/{((DataMemberAttribute)Attribute.GetCustomAttribute(p, typeof(DataMemberAttribute))).Name}",
                        Value = p.GetValue(element) ?? string.Empty,
                    }).ToArray();

            updatedValues.Patch = propertiesToPatch;
            var result = await service.PatchElement(updatedValues);
            return await service.ConvertToDto(result);
        }

        private AObjectiveElementDecorator GetService(string type)
            => type == Constants.ISSUE_TYPE ? issuesService : projectsService;

        private async Task<IEnumerable<string>> GetUpdatedIdsFromService(AObjectiveElementDecorator service, DateTime date)
        {
            var collection = await service.GetAll(date);
            return collection.Select(element => element.GetExternalId());
        }
    }
}
