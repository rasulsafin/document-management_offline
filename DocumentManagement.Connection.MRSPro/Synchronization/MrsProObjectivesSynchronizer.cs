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
        private readonly IElementService issuesService;
        private readonly IElementService projectsService;

        public MrsProObjectivesSynchronizer(IssuesService issuesService, ProjectsService projectsService)
        {
            this.issuesService = issuesService;
            this.projectsService = projectsService;
        }

        public async Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var element = obj.ToModel(obj.ObjectiveType.ExternalId);
            var result = await GetService(element.Type).TryPost(element);
            return result.ToDto();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var objectives = new List<ObjectiveExternalDto>();

            foreach (var id in ids)
            {
                var idParts = id.Split(Constants.ID_SPLITTER);
                (var trueId, var type) = (idParts[0], idParts[1]);
                var result = await GetService(type).TryGetById(trueId);
                objectives.AddIsNotNull(result.ToDto());
            }

            return objectives;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            // TODO: Remove this;
            date = new DateTime(2021, 6, 10);

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
            var element = obj.ToModel(obj.ObjectiveType.ExternalId);
            var updatedValues = new UpdatedValues { Ids = new[] { element.Id } };
            var type = element.GetType();
            var propertiesToPatch = type.GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(IsPatchable)))
                .Select(p => new Patch()
                    {
                        Path = $"/{((DataMemberAttribute)Attribute.GetCustomAttribute(p, typeof(DataMemberAttribute))).Name}",
                        Value = p.GetValue(element) ?? string.Empty,
                    }).ToArray();

            updatedValues.Patch = propertiesToPatch;
            var result = await GetService(element.Type).TryPatch(updatedValues);
            return result.ToDto();
        }

        private IElementService GetService(string type)
        {
            if (type == Constants.ISSUE_TYPE)
                return issuesService;
            else
                return projectsService;
        }

        private async Task<IEnumerable<string>> GetUpdatedIdsFromService(IElementService service, DateTime date)
        {
            var collection = await service.GetAll(date);
            return collection.Select(element => $"{element.Id}{Constants.ID_SPLITTER}{element.Type}");
        }
    }
}
