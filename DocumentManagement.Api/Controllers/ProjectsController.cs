using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private IProjectService service;

        public ProjectsController(IProjectService projectService) => service = projectService;

        [HttpGet]
        [Route("user/{userID}")]
        public async Task<IActionResult> GetUserProjects([FromRoute] int userID)
        {
            var userProjects = await service.GetUserProjects(new ID<UserDto>(userID));
            return ValidateCollection(userProjects);
        }

        [HttpPost]
        [Route("user/{userID}")]
        public async Task<IActionResult> AddToUser([FromRoute] int userID, [FromBody] string title)
        {
            var projectId = await service.AddToUser(new ID<UserDto>(userID), title);
            return ValidateId(projectId);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] string title)
        {
            var projectId = await service.Add(title);
            return ValidateId(projectId);
        }

        [HttpDelete]
        [Route("{projectID}")]
        public async Task<IActionResult> Remove([FromRoute] int projectID)
        {
            var removed = await service.Remove(new ID<ProjectDto>(projectID));
            return ValidateFoundRelatedResult(removed);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProjectDto projectData)
        {
            var updated = await service.Update(projectData);
            return ValidateFoundRelatedResult(updated);
        }

        [HttpGet]
        [Route("{projectID}")]
        public async Task<IActionResult> Find([FromRoute] int projectID)
        {
            var foundProject = await service.Find(new ID<ProjectDto>(projectID));
            return ValidateFoundObject(foundProject);
        }

        [HttpGet]
        [Route("{projectID}/users")]
        public async Task<IActionResult> GetUsers([FromRoute] int projectID)
        {
            var users = await service.GetUsers(new ID<ProjectDto>(projectID));
            return ValidateCollection(users);
        }

        [HttpPost]
        [Route("link/{projectID}")]
        public async Task<IActionResult> LinkToUser([FromRoute] int projectID, [FromBody] IEnumerable<ID<UserDto>> users)
        {
            var added = await service.LinkToUsers(new ID<ProjectDto>(projectID), users);
            return ValidateFoundRelatedResult(added);
        }

        [HttpPost]
        [Route("unlink/{projectID}")]
        public async Task<IActionResult> UnlinkFromUser([FromRoute] int projectID, [FromBody] IEnumerable<ID<UserDto>> users)
        {
            var removed = await service.UnlinkFromUsers(new ID<ProjectDto>(projectID), users);
            return ValidateFoundRelatedResult(removed);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await service.GetAllProjects();
            return ValidateCollection(projects);
        }
    }
}
