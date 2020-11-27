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
        [Route("Get/Objectives")]
        public async Task<IEnumerable<Objective>> GetAllObjectives() => await service.GetAllObjectives();

        [HttpPost]
        [Route("Add/Objective")]
        public async Task<ID<Objective>> Add(ObjectiveToCreate data) => await service.Add(data);

        [HttpDelete]
        [Route("Remove/Objective")]
        public async Task Remove(ID<Objective> objectiveID) => await service.Remove(objectiveID);

        [HttpPut]
        [Route("Update/Objective")]
        public async Task Update(Objective projectData) => await service.Update(projectData);

        [HttpGet]
        [Route("Find/Objective")]
        public async Task<Objective> Find(ID<Objective> objectiveID) => await service.Find(objectiveID);

        [HttpGet]
        [Route("Get/Objectives/Project")]
        public async Task<IEnumerable<Objective>> GetObjectives(ID<Project> projectID) => await service.GetObjectives(projectID);

        [HttpGet]
        [Route("Get/DynamicFields")]
        public async Task<IEnumerable<DynamicFieldInfo>> GetRequiredDynamicFields() => await service.GetRequiredDynamicFields();
    }
}
