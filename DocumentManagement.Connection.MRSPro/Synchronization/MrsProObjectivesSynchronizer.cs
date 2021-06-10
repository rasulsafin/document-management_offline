using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.General.Utils.Extensions;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    /// <summary>
    /// Synchronizes DM.ObjectiveExternalDto models with MrsPro.Objective models.
    /// Merges different types together. Maps models.
    /// </summary>
    public class MrsProObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly IElementService issuesService;
        private readonly IElementService projectsService;

        public MrsProObjectivesSynchronizer(IssuesService issuesService, ProjectsService projectsService)
        {
            this.issuesService = issuesService;
            this.projectsService = projectsService;
        }

        public Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            var element = obj.ToModel(obj.ObjectiveType.ExternalId);
            throw new NotImplementedException();
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
            var projectIds = await GetUpdatedIdsFromService(projectsService, date);
            var objectiveIds = await GetUpdatedIdsFromService(issuesService, date);

            return projectIds.Union(objectiveIds).ToArray();
        }

        public Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public Task<ObjectiveExternalDto> Update(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
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
            return collection.Select(o => o.Id);
        }
    }
}
