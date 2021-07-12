using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.General.Utils.Extensions;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly AElementService issuesService;
        private readonly AElementService projectsService;

        public MrsProObjectivesSynchronizer(IssuesService issuesService, ProjectElementsService projectsService)
        {
            this.issuesService = issuesService;
            this.projectsService = projectsService;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var service = GetService(obj.ObjectiveType.ExternalId);
            var element = await service.ConvertToModel(obj);
            var result = await service.TryPost(element);
            return await service.ConvertToDto(result);
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var objectives = new List<ObjectiveExternalDto>();

            foreach (var id in ids)
            {
                var idParts = id.Split(Constants.ID_SPLITTER);
                (var trueId, var type) = (idParts[0], idParts[1]); // TODO: id is ancestry

                var service = GetService(type);
                var result = await service.TryGetById(trueId);
                objectives.AddIsNotNull(await service.ConvertToDto(result));
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
            var idParts = obj.ExternalID.Split(Constants.ID_SPLITTER);
            (var trueId, var type) = (idParts[0], idParts[1]);

            if (trueId == null)
                throw new Exception("Wrong id value");

            var result = await GetService(type).TryDelete(trueId);

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
            var result = await service.TryPatch(updatedValues);
            return await service.ConvertToDto(result);
        }

        private AElementService GetService(string type)
            => type == Constants.ISSUE_TYPE ? issuesService : projectsService;

        private async Task<IEnumerable<string>> GetUpdatedIdsFromService(IElementService service, DateTime date)
        {
            var collection = await service.GetAll(date);
            return collection.Select(element => $"{element.Id}{Constants.ID_SPLITTER}{element.Type}");
        }
    }
}
