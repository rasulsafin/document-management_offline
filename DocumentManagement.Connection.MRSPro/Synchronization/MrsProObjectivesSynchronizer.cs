using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
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
        private readonly ObjectiveService objectiveService;
        private readonly ProjectsService projectService;

        public MrsProObjectivesSynchronizer(ObjectiveService objectiveService, ProjectsService projectService)
        {
            this.objectiveService = objectiveService;
            this.projectService = projectService;
        }

        public Task<ObjectiveExternalDto> Add(ObjectiveExternalDto obj)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var objectives = new List<ObjectiveExternalDto>();

            foreach (var id in ids)
            {
                IElement result = await projectService.TryGetProjectById(id);
                if (result == null)
                    result = await objectiveService.TryGetObjectiveById(id);

                objectives.AddIsNotNull(result.ToObjectiveExternalDto());
            }

            return objectives;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            var listOfObjectives = await objectiveService.GetObjectives(date);
            var listOfProjects = await projectService.GetProjects();
            var objectiveIds = listOfObjectives.Select(o => o.Id);
            var projectIds = listOfProjects.Select(o => o.Id);

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
    }
}
