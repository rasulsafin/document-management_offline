using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<IEnumerable<ProjectDto>> GetUserProjects([FromRoute] int userID) => await service.GetUserProjects(new ID<UserDto>(userID));

        [HttpPost]
        [Route("user/{userID}")]
        public async Task<ID<ProjectDto>> Add([FromRoute] int userID, [FromBody] string title) => await service.Add(new ID<UserDto>(userID), title);

        [HttpDelete]
        [Route("{projectID}")]
        public async Task Remove([FromRoute] int projectID) => await service.Remove(new ID<ProjectDto>(projectID));

        [HttpPut]
        public async Task Update([FromBody] ProjectDto projectData) => await service.Update(projectData);

        [HttpGet]
        [Route("{projectID}")]
        public async Task<ProjectDto> Find([FromRoute] int projectID) => await service.Find(new ID<ProjectDto>(projectID));

        [HttpGet]
        [Route("{projectID}/users")]
        public async Task<IEnumerable<UserDto>> GetUsers([FromRoute] int projectID) => await service.GetUsers(new ID<ProjectDto>(projectID));

        [HttpPost]
        [Route("{projectID}/users")]
        public async Task AddUsers([FromRoute] int projectID, [FromBody] IEnumerable<ID<UserDto>> users) => await service.AddUsers(new ID<ProjectDto>(projectID), users);

        [HttpDelete]
        [Route("{projectID}/users")]
        public async Task RemoveUsers([FromRoute] int projectID, [FromBody] IEnumerable<ID<UserDto>> users) => await service.RemoveUsers(new ID<ProjectDto>(projectID), users);
    }
}
