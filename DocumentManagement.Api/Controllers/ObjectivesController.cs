using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ObjectivesController : ControllerBase
    {
        private IObjectiveService service;

        public ObjectivesController(IObjectiveService objectiveService) => service = objectiveService;

        [HttpGet]
        public async Task<IEnumerable<Objective>> GetAllObjectives() => await service.GetAllObjectives();

        [HttpPost]
        public async Task<ID<Objective>> Add(ObjectiveToCreate data) => await service.Add(data);

        [HttpDelete]
        public async Task Remove(ID<Objective> objectiveID) => await service.Remove(objectiveID);

        [HttpPut]
        public async Task Update(Objective projectData) => await service.Update(projectData);

        [HttpGet]
        public async Task<Objective> Find(ID<Objective> objectiveID) => await service.Find(objectiveID);

        [HttpGet]
        public async Task<IEnumerable<Objective>> GetObjectives(ID<Project> projectID) => await service.GetObjectives(projectID);

        [HttpGet]
        public async Task<IEnumerable<DynamicFieldInfo>> GetRequiredDynamicFields() => await service.GetRequiredDynamicFields();
    }
}
