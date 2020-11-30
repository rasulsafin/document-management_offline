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
        public async Task<IEnumerable<ProjectDto>> GetUserProjects(ID<UserDto> userID) => await service.GetUserProjects(userID);

        [HttpPost]
        public async Task<ID<ProjectDto>> Add(ID<UserDto> owner, string title) => await service.Add(owner, title);

        [HttpDelete]
        public async Task Remove(ID<ProjectDto> projectID) => await service.Remove(projectID);

        [HttpPut]
        public async Task Update(ProjectDto projectData) => await service.Update(projectData);

        [HttpGet]
        [Route("find/project")]
        public async Task<ProjectDto> Find(ID<ProjectDto> projectID) => await service.Find(projectID);

        [HttpGet]
        [Route("users")]
        public async Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID) => await service.GetUsers(projectID);

        [HttpPost]
        [Route("users")]
        public async Task AddUsers([FromHeader] ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users) => await service.AddUsers(projectID, users);

        [HttpDelete]
        [Route("users")]
        public async Task RemoveUsers([FromHeader] ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users) => await service.RemoveUsers(projectID, users);
    }
}
